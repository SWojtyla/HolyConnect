using System.Net;
using HolyConnect.Domain.Entities;
using HolyConnect.Infrastructure.Services;
using Moq;
using Moq.Protected;

namespace HolyConnect.Infrastructure.Tests.Services;

public class RestRequestExecutorTests
{
    [Fact]
    public void CanExecute_ShouldReturnTrue_ForRestRequest()
    {
        // Arrange
        var httpClient = new HttpClient();
        var executor = new RestRequestExecutor(httpClient);
        var request = new RestRequest();

        // Act
        var result = executor.CanExecute(request);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanExecute_ShouldReturnFalse_ForNonRestRequest()
    {
        // Arrange
        var httpClient = new HttpClient();
        var executor = new RestRequestExecutor(httpClient);
        var request = new GraphQLRequest();

        // Act
        var result = executor.CanExecute(request);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowException_ForNonRestRequest()
    {
        // Arrange
        var httpClient = new HttpClient();
        var executor = new RestRequestExecutor(httpClient);
        var request = new GraphQLRequest();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => executor.ExecuteAsync(request));
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnResponse_WithStatusCode()
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
                Content = new StringContent("{\"message\": \"success\"}")
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var executor = new RestRequestExecutor(httpClient);
        var request = new RestRequest
        {
            Url = "https://api.example.com/test",
            Method = Domain.Entities.HttpMethod.Get
        };

        // Act
        var response = await executor.ExecuteAsync(request);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(response.Body);
        Assert.Contains("success", response.Body);
        Assert.True(response.ResponseTime >= 0);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldIncludeHeaders()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        var responseMessage = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("test")
        };
        responseMessage.Headers.Add("X-Custom-Header", "CustomValue");

        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        var httpClient = new HttpClient(mockHandler.Object);
        var executor = new RestRequestExecutor(httpClient);
        var request = new RestRequest
        {
            Url = "https://api.example.com/test",
            Method = Domain.Entities.HttpMethod.Get
        };

        // Act
        var response = await executor.ExecuteAsync(request);

        // Assert
        Assert.True(response.Headers.ContainsKey("X-Custom-Header"));
        Assert.Equal("CustomValue", response.Headers["X-Custom-Header"]);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldHandleException()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        var httpClient = new HttpClient(mockHandler.Object);
        var executor = new RestRequestExecutor(httpClient);
        var request = new RestRequest
        {
            Url = "https://api.example.com/test",
            Method = Domain.Entities.HttpMethod.Get
        };

        // Act
        var response = await executor.ExecuteAsync(request);

        // Assert
        Assert.Equal(0, response.StatusCode);
        Assert.Contains("Error", response.StatusMessage);
        Assert.Contains("Network error", response.Body);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCaptureSentRequest()
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
                Content = new StringContent("{\"message\": \"success\"}")
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var executor = new RestRequestExecutor(httpClient);
        var request = new RestRequest
        {
            Url = "https://api.example.com/test",
            Method = Domain.Entities.HttpMethod.Post,
            Body = "{\"test\": \"data\"}",
            Headers = { { "Authorization", "Bearer token123" } }
        };

        // Act
        var response = await executor.ExecuteAsync(request);

        // Assert
        Assert.NotNull(response.SentRequest);
        Assert.Equal("Post", response.SentRequest.Method);
        Assert.Contains("api.example.com/test", response.SentRequest.Url);
        Assert.Equal("{\"test\": \"data\"}", response.SentRequest.Body);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCaptureSentRequestWithQueryParameters()
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
                Content = new StringContent("test")
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var executor = new RestRequestExecutor(httpClient);
        var request = new RestRequest
        {
            Url = "https://api.example.com/test",
            Method = Domain.Entities.HttpMethod.Get,
            QueryParameters = { { "page", "1" }, { "limit", "10" } }
        };

        // Act
        var response = await executor.ExecuteAsync(request);

        // Assert
        Assert.NotNull(response.SentRequest);
        Assert.Equal(2, response.SentRequest.QueryParameters.Count);
        Assert.Equal("1", response.SentRequest.QueryParameters["page"]);
        Assert.Equal("10", response.SentRequest.QueryParameters["limit"]);
    }
}
