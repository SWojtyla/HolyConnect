namespace HolyConnect.Domain.Entities;

/// <summary>
/// Represents a historical entry of a request execution, including the request and response
/// </summary>
public class RequestHistoryEntry
{
    public Guid Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string RequestName { get; set; } = string.Empty;
    public RequestType RequestType { get; set; }
    public SentRequest SentRequest { get; set; } = new();
    public RequestResponse Response { get; set; } = new();
}
