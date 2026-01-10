using HolyConnect.Domain.Entities;

namespace HolyConnect.Application.Interfaces;

/// <summary>
/// Service for managing collections (independent of environments).
/// </summary>
public interface ICollectionService
{
    Task<Collection> CreateCollectionAsync(string name, Guid? parentCollectionId = null, string? description = null);
    Task<IEnumerable<Collection>> GetAllCollectionsAsync();
    Task<Collection?> GetCollectionByIdAsync(Guid id);
    Task<Collection> UpdateCollectionAsync(Collection collection);
    Task DeleteCollectionAsync(Guid id);
    
    /// <summary>
    /// Updates the order index of multiple collections in a single operation.
    /// </summary>
    Task UpdateCollectionOrderAsync(IEnumerable<(Guid CollectionId, int OrderIndex)> collectionOrders);
    
    /// <summary>
    /// Moves a collection up or down in the order (within the same parent).
    /// </summary>
    Task MoveCollectionAsync(Guid collectionId, bool moveUp);
}
