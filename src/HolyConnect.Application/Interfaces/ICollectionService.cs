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
    Task MoveCollectionAsync(Guid collectionId, Guid? newParentCollectionId, int newOrder);
}
