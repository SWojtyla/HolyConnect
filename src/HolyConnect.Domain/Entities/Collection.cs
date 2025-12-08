namespace HolyConnect.Domain.Entities;

public class Collection
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Dictionary<string, string> Variables { get; set; } = new();
    public Guid? ParentCollectionId { get; set; }
    public Collection? ParentCollection { get; set; }
    public List<Collection> SubCollections { get; set; } = new();
    public List<Request> Requests { get; set; } = new();
    public Guid EnvironmentId { get; set; }
    public Environment? Environment { get; set; }
    public DateTime CreatedAt { get; set; }
}
