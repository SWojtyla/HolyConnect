using HolyConnect.Application.Interfaces;
using HolyConnect.Domain.Entities;

namespace HolyConnect.Application.Services;

public class CollectionService : ICollectionService
{
    private readonly IRepository<Collection> _collectionRepository;

    public CollectionService(IRepository<Collection> collectionRepository)
    {
        _collectionRepository = collectionRepository;
    }

    public async Task<Collection> CreateCollectionAsync(string name, Guid environmentId, Guid? parentCollectionId = null, string? description = null)
    {
        var collection = new Collection
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            EnvironmentId = environmentId,
            ParentCollectionId = parentCollectionId,
            CreatedAt = DateTime.UtcNow
        };

        return await _collectionRepository.AddAsync(collection);
    }

    public async Task<IEnumerable<Collection>> GetAllCollectionsAsync()
    {
        return await _collectionRepository.GetAllAsync();
    }

    public async Task<Collection?> GetCollectionByIdAsync(Guid id)
    {
        return await _collectionRepository.GetByIdAsync(id);
    }

    public async Task<Collection> UpdateCollectionAsync(Collection collection)
    {
        return await _collectionRepository.UpdateAsync(collection);
    }

    public async Task DeleteCollectionAsync(Guid id)
    {
        await _collectionRepository.DeleteAsync(id);
    }
}
