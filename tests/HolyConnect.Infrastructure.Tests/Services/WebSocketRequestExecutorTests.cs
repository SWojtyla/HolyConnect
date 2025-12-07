using HolyConnect.Domain.Entities;
using HolyConnect.Infrastructure.Services;

namespace HolyConnect.Infrastructure.Tests.Services;

public class WebSocketRequestExecutorTests
{
    [Fact]
    public void CanExecute_ShouldReturnTrue_ForStandardWebSocketRequest()
    {
        // Arrange
        var executor = new WebSocketRequestExecutor();
        var request = new WebSocketRequest
        {
            ConnectionType = WebSocketConnectionType.Standard
        };

        // Act
        var result = executor.CanExecute(request);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanExecute_ShouldReturnFalse_ForGraphQLSubscriptionWebSocketRequest()
    {
        // Arrange
        var executor = new WebSocketRequestExecutor();
        var request = new WebSocketRequest
        {
            ConnectionType = WebSocketConnectionType.GraphQLSubscription
        };

        // Act
        var result = executor.CanExecute(request);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CanExecute_ShouldReturnFalse_ForNonWebSocketRequest()
    {
        // Arrange
        var executor = new WebSocketRequestExecutor();
        var request = new RestRequest();

        // Act
        var result = executor.CanExecute(request);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowException_ForNonWebSocketRequest()
    {
        // Arrange
        var executor = new WebSocketRequestExecutor();
        var request = new RestRequest();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => executor.ExecuteAsync(request));
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnStreamingResponse()
    {
        // Arrange
        var executor = new WebSocketRequestExecutor();
        var request = new WebSocketRequest
        {
            Url = "wss://echo.websocket.org/",
            Message = "Test message"
        };

        // Act
        var response = await executor.ExecuteAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.IsStreaming);
        Assert.NotNull(response.StreamEvents);
        Assert.NotNull(response.SentRequest);
        Assert.Equal("WEBSOCKET", response.SentRequest.Method);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldHandleInvalidUrl()
    {
        // Arrange
        var executor = new WebSocketRequestExecutor();
        var request = new WebSocketRequest
        {
            Url = "invalid-url"
        };

        // Act
        var response = await executor.ExecuteAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(0, response.StatusCode);
        Assert.Contains("Error", response.StatusMessage);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldApplyBasicAuthentication()
    {
        // Arrange
        var executor = new WebSocketRequestExecutor();
        var request = new WebSocketRequest
        {
            Url = "wss://echo.websocket.org/",
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
        var executor = new WebSocketRequestExecutor();
        var request = new WebSocketRequest
        {
            Url = "wss://echo.websocket.org/",
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
    public async Task ExecuteAsync_ShouldIncludeProtocols()
    {
        // Arrange
        var executor = new WebSocketRequestExecutor();
        var request = new WebSocketRequest
        {
            Url = "wss://echo.websocket.org/",
            Protocols = new List<string> { "protocol1", "protocol2" }
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
        var executor = new WebSocketRequestExecutor();
        var request = new WebSocketRequest
        {
            Url = "wss://echo.websocket.org/",
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
