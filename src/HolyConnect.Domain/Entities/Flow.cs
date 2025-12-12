namespace HolyConnect.Domain.Entities;

/// <summary>
/// Represents a sequence of requests that can be executed one after another.
/// Each step can extract values from responses and pass them as variables to subsequent steps.
/// Flows are now independent of environments and optionally belong to a collection.
/// </summary>
public class Flow
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? CollectionId { get; set; }
    public Collection? Collection { get; set; }
    public List<FlowStep> Steps { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}
