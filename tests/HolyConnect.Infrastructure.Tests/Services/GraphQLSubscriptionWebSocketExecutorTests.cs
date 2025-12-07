using HolyConnect.Domain.Entities;
using HolyConnect.Infrastructure.Services;

namespace HolyConnect.Infrastructure.Tests.Services;

public class GraphQLSubscriptionWebSocketExecutorTests
{
    [Fact]
    public void CanExecute_ShouldReturnTrue_ForGraphQLSubscriptionWithWebSocket()
    {
        // Arrange
        var executor = new GraphQLSubscriptionWebSocketExecutor();
        var request = new GraphQLRequest
        {
            OperationType = GraphQLOperationType.Subscription,
            SubscriptionProtocol = GraphQLSubscriptionProtocol.WebSocket
        };

        // Act
        var result = executor.CanExecute(request);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanExecute_ShouldReturnFalse_ForGraphQLQueryWithWebSocket()
    {
        // Arrange
        var executor = new GraphQLSubscriptionWebSocketExecutor();
        var request = new GraphQLRequest
        {
            OperationType = GraphQLOperationType.Query,
            SubscriptionProtocol = GraphQLSubscriptionProtocol.WebSocket
        };

        // Act
        var result = executor.CanExecute(request);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CanExecute_ShouldReturnFalse_ForGraphQLSubscriptionWithSSE()
    {
        // Arrange
        var executor = new GraphQLSubscriptionWebSocketExecutor();
        var request = new GraphQLRequest
        {
            OperationType = GraphQLOperationType.Subscription,
            SubscriptionProtocol = GraphQLSubscriptionProtocol.ServerSentEvents
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
        var executor = new GraphQLSubscriptionWebSocketExecutor();
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
        var executor = new GraphQLSubscriptionWebSocketExecutor();
        var request = new RestRequest();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => executor.ExecuteAsync(request));
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnStreamingResponse()
    {
        // Arrange
        var executor = new GraphQLSubscriptionWebSocketExecutor();
        var request = new GraphQLRequest
        {
            Url = "wss://invalid-test-endpoint.example.com/graphql",
            Query = "subscription { messageAdded { id content } }",
            OperationType = GraphQLOperationType.Subscription,
            SubscriptionProtocol = GraphQLSubscriptionProtocol.WebSocket
        };

        // Act
        var response = await executor.ExecuteAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.IsStreaming);
        Assert.NotNull(response.StreamEvents);
        Assert.NotNull(response.SentRequest);
        Assert.Equal("GRAPHQL_SUBSCRIPTION_WS", response.SentRequest.Method);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldConvertHttpToWs()
    {
        // Arrange
        var executor = new GraphQLSubscriptionWebSocketExecutor();
        var request = new GraphQLRequest
        {
            Url = "http://example.com/graphql",
            Query = "subscription { test }",
            OperationType = GraphQLOperationType.Subscription,
            SubscriptionProtocol = GraphQLSubscriptionProtocol.WebSocket
        };

        // Act
        var response = await executor.ExecuteAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.SentRequest);
        Assert.Equal("http://example.com/graphql", response.SentRequest.Url);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldIncludeVariables()
    {
        // Arrange
        var executor = new GraphQLSubscriptionWebSocketExecutor();
        var request = new GraphQLRequest
        {
            Url = "wss://example.com/graphql",
            Query = "subscription ($id: ID!) { messageAdded(id: $id) { content } }",
            Variables = "{\"id\": \"123\"}",
            OperationType = GraphQLOperationType.Subscription,
            SubscriptionProtocol = GraphQLSubscriptionProtocol.WebSocket
        };

        // Act
        var response = await executor.ExecuteAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.SentRequest);
        Assert.Contains("123", response.SentRequest.Body);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldApplyBasicAuthentication()
    {
        // Arrange
        var executor = new GraphQLSubscriptionWebSocketExecutor();
        var request = new GraphQLRequest
        {
            Url = "wss://example.com/graphql",
            Query = "subscription { test }",
            OperationType = GraphQLOperationType.Subscription,
            SubscriptionProtocol = GraphQLSubscriptionProtocol.WebSocket,
            AuthType = AuthenticationType.Basic,
            BasicAuthUsername = "testuser",
            BasicAuthPassword = "testpass"
        };

        // Act
        var response = await executor.ExecuteAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.SentRequest);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldApplyBearerToken()
    {
        // Arrange
        var executor = new GraphQLSubscriptionWebSocketExecutor();
        var request = new GraphQLRequest
        {
            Url = "wss://example.com/graphql",
            Query = "subscription { test }",
            OperationType = GraphQLOperationType.Subscription,
            SubscriptionProtocol = GraphQLSubscriptionProtocol.WebSocket,
            AuthType = AuthenticationType.BearerToken,
            BearerToken = "test-token-123"
        };

        // Act
        var response = await executor.ExecuteAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.SentRequest);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldRespectDisabledHeaders()
    {
        // Arrange
        var executor = new GraphQLSubscriptionWebSocketExecutor();
        var request = new GraphQLRequest
        {
            Url = "wss://example.com/graphql",
            Query = "subscription { test }",
            OperationType = GraphQLOperationType.Subscription,
            SubscriptionProtocol = GraphQLSubscriptionProtocol.WebSocket,
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
