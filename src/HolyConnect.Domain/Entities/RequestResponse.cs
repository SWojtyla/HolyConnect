namespace HolyConnect.Domain.Entities;

public class RequestResponse
{
    public int StatusCode { get; set; }
    public string StatusMessage { get; set; } = string.Empty;
    public Dictionary<string, string> Headers { get; set; } = new();
    public string Body { get; set; } = string.Empty;
    public long ResponseTime { get; set; }
    public long Size { get; set; }
    public DateTime Timestamp { get; set; }
    public SentRequest? SentRequest { get; set; }
}

public class SentRequest
{
    public string Url { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public Dictionary<string, string> Headers { get; set; } = new();
    public string? Body { get; set; }
    public Dictionary<string, string> QueryParameters { get; set; } = new();
}
