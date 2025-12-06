using HolyConnect.Domain.Entities;

namespace HolyConnect.Application.Interfaces;

public interface ICollectionService
{
    Task<Collection> CreateCollectionAsync(string name, Guid environmentId, Guid? parentCollectionId = null, string? description = null);
    Task<IEnumerable<Collection>> GetAllCollectionsAsync();
    Task<Collection?> GetCollectionByIdAsync(Guid id);
    Task<Collection> UpdateCollectionAsync(Collection collection);
    Task DeleteCollectionAsync(Guid id);
}
