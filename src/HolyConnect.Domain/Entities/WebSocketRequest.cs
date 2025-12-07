namespace HolyConnect.Domain.Entities;

public class WebSocketRequest : Request
{
    public string? Message { get; set; }
    public List<string> Protocols { get; set; } = new();
    public override RequestType Type => RequestType.WebSocket;
    public WebSocketConnectionType ConnectionType { get; set; } = WebSocketConnectionType.Standard;
}

public enum WebSocketConnectionType
{
    Standard,
    GraphQLSubscription
}
