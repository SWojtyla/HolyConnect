namespace HolyConnect.Domain.Entities;

/// <summary>
/// Represents a historical entry of a request execution, including the request and response.
/// EnvironmentId is optional and indicates which environment was active when the request was executed.
/// </summary>
public class RequestHistoryEntry
{
    public Guid Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string RequestName { get; set; } = string.Empty;
    public RequestType RequestType { get; set; }
    public SentRequest SentRequest { get; set; } = new();
    public RequestResponse Response { get; set; } = new();
    
    // Navigation properties to link back to the original request
    public Guid? RequestId { get; set; }
    public Guid? EnvironmentId { get; set; }  // Which environment was active during execution
    public Guid? CollectionId { get; set; }
}
