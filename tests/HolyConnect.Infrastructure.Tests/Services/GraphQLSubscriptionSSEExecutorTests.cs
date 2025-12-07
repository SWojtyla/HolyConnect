using System.Net;
using HolyConnect.Domain.Entities;
using HolyConnect.Infrastructure.Services;
using Moq;
using Moq.Protected;

namespace HolyConnect.Infrastructure.Tests.Services;

public class GraphQLSubscriptionSSEExecutorTests
{
    [Fact]
    public void CanExecute_ShouldReturnTrue_ForGraphQLSubscriptionWithSSE()
    {
        // Arrange
        var httpClient = new HttpClient();
        var executor = new GraphQLSubscriptionSSEExecutor(httpClient);
        var request = new GraphQLRequest
        {
            OperationType = GraphQLOperationType.Subscription,
            SubscriptionProtocol = GraphQLSubscriptionProtocol.ServerSentEvents
        };

        // Act
        var result = executor.CanExecute(request);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanExecute_ShouldReturnFalse_ForGraphQLQueryWithSSE()
    {
        // Arrange
        var httpClient = new HttpClient();
        var executor = new GraphQLSubscriptionSSEExecutor(httpClient);
        var request = new GraphQLRequest
        {
            OperationType = GraphQLOperationType.Query,
            SubscriptionProtocol = GraphQLSubscriptionProtocol.ServerSentEvents
        };

        // Act
        var result = executor.CanExecute(request);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CanExecute_ShouldReturnFalse_ForGraphQLSubscriptionWithWebSocket()
    {
        // Arrange
        var httpClient = new HttpClient();
        var executor = new GraphQLSubscriptionSSEExecutor(httpClient);
        var request = new GraphQLRequest
        {
            OperationType = GraphQLOperationType.Subscription,
            SubscriptionProtocol = GraphQLSubscriptionProtocol.WebSocket
        };

        // Act
        var result = executor.CanExecute(request);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CanExecute_ShouldReturnFalse_ForNonGraphQLRequest()
    {
        // Arrange
        var httpClient = new HttpClient();
        var executor = new GraphQLSubscriptionSSEExecutor(httpClient);
        var request = new RestRequest();

        // Act
        var result = executor.CanExecute(request);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowException_ForNonGraphQLRequest()
    {
        // Arrange
        var httpClient = new HttpClient();
        var executor = new GraphQLSubscriptionSSEExecutor(httpClient);
        var request = new RestRequest();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => executor.ExecuteAsync(request));
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnStreamingResponse()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        var sseContent = "event: message\ndata: {\"data\": {\"test\": \"value\"}}\n\n";
        
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(sseContent)
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var executor = new GraphQLSubscriptionSSEExecutor(httpClient);
        var request = new GraphQLRequest
        {
            Url = "https://example.com/graphql",
            Query = "subscription { test }",
            OperationType = GraphQLOperationType.Subscription,
            SubscriptionProtocol = GraphQLSubscriptionProtocol.ServerSentEvents
        };

        // Act
        var response = await executor.ExecuteAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.IsStreaming);
        Assert.NotNull(response.StreamEvents);
        Assert.NotNull(response.SentRequest);
        Assert.Equal("GRAPHQL_SUBSCRIPTION_SSE", response.SentRequest.Method);
        Assert.Equal(200, response.StatusCode);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldIncludeVariables()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("")
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var executor = new GraphQLSubscriptionSSEExecutor(httpClient);
        var request = new GraphQLRequest
        {
            Url = "https://example.com/graphql",
            Query = "subscription ($id: ID!) { test(id: $id) }",
            Variables = "{\"id\": \"123\"}",
            OperationType = GraphQLOperationType.Subscription,
            SubscriptionProtocol = GraphQLSubscriptionProtocol.ServerSentEvents
        };

        // Act
        var response = await executor.ExecuteAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.SentRequest);
        Assert.Contains("123", response.SentRequest.Body);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldParseSSEEvents()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        var sseContent = "event: data\ndata: first event\n\nevent: data\ndata: second event\n\n";
        
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(sseContent)
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var executor = new GraphQLSubscriptionSSEExecutor(httpClient);
        var request = new GraphQLRequest
        {
            Url = "https://example.com/graphql",
            Query = "subscription { test }",
            OperationType = GraphQLOperationType.Subscription,
            SubscriptionProtocol = GraphQLSubscriptionProtocol.ServerSentEvents
        };

        // Act
        var response = await executor.ExecuteAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.NotEmpty(response.StreamEvents);
        Assert.Equal(2, response.StreamEvents.Count);
        Assert.Equal("first event", response.StreamEvents[0].Data);
        Assert.Equal("second event", response.StreamEvents[1].Data);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldApplyBasicAuthentication()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("")
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var executor = new GraphQLSubscriptionSSEExecutor(httpClient);
        var request = new GraphQLRequest
        {
            Url = "https://example.com/graphql",
            Query = "subscription { test }",
            OperationType = GraphQLOperationType.Subscription,
            SubscriptionProtocol = GraphQLSubscriptionProtocol.ServerSentEvents,
            AuthType = AuthenticationType.Basic,
            BasicAuthUsername = "testuser",
            BasicAuthPassword = "testpass"
        };

        // Act
        var response = await executor.ExecuteAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.SentRequest);
        Assert.Equal(200, response.StatusCode);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldApplyBearerToken()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("")
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var executor = new GraphQLSubscriptionSSEExecutor(httpClient);
        var request = new GraphQLRequest
        {
            Url = "https://example.com/graphql",
            Query = "subscription { test }",
            OperationType = GraphQLOperationType.Subscription,
            SubscriptionProtocol = GraphQLSubscriptionProtocol.ServerSentEvents,
            AuthType = AuthenticationType.BearerToken,
            BearerToken = "test-token-123"
        };

        // Act
        var response = await executor.ExecuteAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.SentRequest);
        Assert.Equal(200, response.StatusCode);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldRespectDisabledHeaders()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("")
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var executor = new GraphQLSubscriptionSSEExecutor(httpClient);
        var request = new GraphQLRequest
        {
            Url = "https://example.com/graphql",
            Query = "subscription { test }",
            OperationType = GraphQLOperationType.Subscription,
            SubscriptionProtocol = GraphQLSubscriptionProtocol.ServerSentEvents,
            Headers = new Dictionary<string, string>
            {
                { "X-Custom-Header", "value1" },
                { "X-Disabled-Header", "value2" }
            },
            DisabledHeaders = new HashSet<string> { "X-Disabled-Header" }
        };

        // Act
        var response = await executor.ExecuteAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.SentRequest);
        Assert.True(response.SentRequest.Headers.ContainsKey("X-Custom-Header"));
        Assert.False(response.SentRequest.Headers.ContainsKey("X-Disabled-Header"));
    }
}
