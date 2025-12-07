using HolyConnect.Domain.Entities;

namespace HolyConnect.Domain.Tests.Entities;

public class WebSocketRequestTests
{
    [Fact]
    public void Type_ShouldReturnWebSocket()
    {
        // Arrange & Act
        var request = new WebSocketRequest();

        // Assert
        Assert.Equal(RequestType.WebSocket, request.Type);
    }

    [Fact]
    public void Constructor_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var request = new WebSocketRequest();

        // Assert
        Assert.NotNull(request.Protocols);
        Assert.Empty(request.Protocols);
        Assert.Null(request.Message);
        Assert.Equal(WebSocketConnectionType.Standard, request.ConnectionType);
    }

    [Fact]
    public void Message_ShouldBeSettable()
    {
        // Arrange
        var request = new WebSocketRequest();
        var message = "Hello WebSocket";

        // Act
        request.Message = message;

        // Assert
        Assert.Equal(message, request.Message);
    }

    [Fact]
    public void Protocols_ShouldAllowAddingProtocols()
    {
        // Arrange
        var request = new WebSocketRequest();

        // Act
        request.Protocols.Add("graphql-ws");
        request.Protocols.Add("graphql-transport-ws");

        // Assert
        Assert.Equal(2, request.Protocols.Count);
        Assert.Contains("graphql-ws", request.Protocols);
        Assert.Contains("graphql-transport-ws", request.Protocols);
    }

    [Fact]
    public void ConnectionType_ShouldBeSettable()
    {
        // Arrange
        var request = new WebSocketRequest();

        // Act
        request.ConnectionType = WebSocketConnectionType.GraphQLSubscription;

        // Assert
        Assert.Equal(WebSocketConnectionType.GraphQLSubscription, request.ConnectionType);
    }

    [Fact]
    public void InheritsFrom_Request()
    {
        // Arrange
        var request = new WebSocketRequest
        {
            Id = Guid.NewGuid(),
            Name = "Test WebSocket",
            Url = "wss://example.com/ws"
        };

        // Assert
        Assert.IsAssignableFrom<Request>(request);
        Assert.NotEqual(Guid.Empty, request.Id);
        Assert.Equal("Test WebSocket", request.Name);
        Assert.Equal("wss://example.com/ws", request.Url);
    }
}
