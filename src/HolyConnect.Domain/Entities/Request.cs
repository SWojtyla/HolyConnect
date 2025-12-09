namespace HolyConnect.Domain.Entities;

public abstract class Request
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Url { get; set; } = string.Empty;
    public Dictionary<string, string> Headers { get; set; } = new();
    public HashSet<string> DisabledHeaders { get; set; } = new();
    public List<DynamicVariable> DynamicVariables { get; set; } = new();
    public Guid? CollectionId { get; set; }
    public Collection? Collection { get; set; }
    public Guid EnvironmentId { get; set; }
    public Environment? Environment { get; set; }
    public DateTime CreatedAt { get; set; }
    public abstract RequestType Type { get; }
    public AuthenticationType AuthType { get; set; } = AuthenticationType.None;
    public string? BasicAuthUsername { get; set; }
    public string? BasicAuthPassword { get; set; }
    public string? BearerToken { get; set; }
    public List<ResponseExtraction> ResponseExtractions { get; set; } = new();
}

public enum RequestType
{
    Rest,
    GraphQL,
    Soap,
    WebSocket
}

public enum AuthenticationType
{
    None,
    Basic,
    BearerToken
}
