namespace HolyConnect.Domain.Entities;

public class RestRequest : Request
{
    public HttpMethod Method { get; set; } = HttpMethod.Get;
    public string? Body { get; set; }
    public string? ContentType { get; set; }
    public BodyType BodyType { get; set; } = BodyType.Json;
    public Dictionary<string, string> QueryParameters { get; set; } = new();
    public HashSet<string> DisabledQueryParameters { get; set; } = new();
    public override RequestType Type => RequestType.Rest;
}

public enum HttpMethod
{
    Get,
    Post,
    Put,
    Delete,
    Patch,
    Head,
    Options
}

public enum BodyType
{
    None,
    Json,
    Xml,
    Text,
    Html,
    JavaScript
}
