using HolyConnect.Domain.Entities;

namespace HolyConnect.Maui.Models;

/// <summary>
/// Represents an item that can be dragged in the tree view.
/// Can be either a collection or a request.
/// </summary>
public class DraggableItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DraggableItemType Type { get; set; }
    public Guid? ParentId { get; set; }
    public int Order { get; set; }
    public object Item { get; set; } = null!;
}

public enum DraggableItemType
{
    Collection,
    Request
}
