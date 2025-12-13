using HolyConnect.Application.Common;
using HolyConnect.Application.Interfaces;
using HolyConnect.Domain.Entities;

namespace HolyConnect.Application.Services;

/// <summary>
/// Service for managing collections (independent of environments).
/// </summary>
public class CollectionService : ICollectionService
{
    private readonly IRepository<Collection> _collectionRepository;
    private readonly ISecretVariablesService _secretVariablesService;

    public CollectionService(
        IRepository<Collection> collectionRepository,
        ISecretVariablesService secretVariablesService)
    {
        _collectionRepository = collectionRepository;
        _secretVariablesService = secretVariablesService;
    }

    public async Task<Collection> CreateCollectionAsync(string name, Guid? parentCollectionId = null, string? description = null)
    {
        var collection = new Collection
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
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
        var collection = await _collectionRepository.GetByIdAsync(id);
        if (collection != null)
        {
            // Load secret variable values and merge them into the Variables dictionary
            await SecretVariableHelper.LoadAndMergeSecretsAsync(
                id,
                collection.Variables,
                _secretVariablesService.GetCollectionSecretsAsync);
        }
        return collection;
    }

    public async Task<Collection> UpdateCollectionAsync(Collection collection)
    {
        // Separate secret and non-secret variables
        var separated = SecretVariableHelper.SeparateVariables(
            collection.Variables,
            collection.SecretVariableNames);
        
        // Save secrets separately
        await _secretVariablesService.SaveCollectionSecretsAsync(collection.Id, separated.SecretVariables);
        
        // Update collection with only non-secret variables
        collection.Variables = separated.NonSecretVariables;
        var result = await _collectionRepository.UpdateAsync(collection);
        
        // Restore all variables (including secrets) for the returned object
        SecretVariableHelper.MergeSecretVariables(result.Variables, separated.SecretVariables);
        
        return result;
    }

    public async Task DeleteCollectionAsync(Guid id)
    {
        await _secretVariablesService.DeleteCollectionSecretsAsync(id);
        await _collectionRepository.DeleteAsync(id);
    }
}
