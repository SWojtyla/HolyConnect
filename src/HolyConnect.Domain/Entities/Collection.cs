namespace HolyConnect.Domain.Entities;

/// <summary>
/// Represents a hierarchical collection for organizing requests.
/// Collections are now independent of environments and only used for grouping requests.
/// </summary>
public class Collection
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Dictionary<string, string> Variables { get; set; } = new();
    public HashSet<string> SecretVariableNames { get; set; } = new();
    public List<DynamicVariable> DynamicVariables { get; set; } = new();
    public Guid? ParentCollectionId { get; set; }
    public Collection? ParentCollection { get; set; }
    public List<Collection> SubCollections { get; set; } = new();
    public List<Request> Requests { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// The order index for sorting collections within the same parent.
    /// Lower values appear first. Default is 0.
    /// </summary>
    public int OrderIndex { get; set; } = 0;
}
