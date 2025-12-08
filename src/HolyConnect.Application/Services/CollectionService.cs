using HolyConnect.Application.Interfaces;
using HolyConnect.Domain.Entities;

namespace HolyConnect.Application.Services;

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
        var collection = await _collectionRepository.GetByIdAsync(id);
        if (collection != null)
        {
            // Load secret variable values and merge them into the Variables dictionary
            var secrets = await _secretVariablesService.GetCollectionSecretsAsync(id);
            foreach (var secret in secrets)
            {
                collection.Variables[secret.Key] = secret.Value;
            }
        }
        return collection;
    }

    public async Task<Collection> UpdateCollectionAsync(Collection collection)
    {
        // Separate secret and non-secret variables
        var secretVariables = new Dictionary<string, string>();
        var nonSecretVariables = new Dictionary<string, string>();
        
        foreach (var variable in collection.Variables)
        {
            if (collection.SecretVariableNames.Contains(variable.Key))
            {
                secretVariables[variable.Key] = variable.Value;
            }
            else
            {
                nonSecretVariables[variable.Key] = variable.Value;
            }
        }
        
        // Save secrets separately
        await _secretVariablesService.SaveCollectionSecretsAsync(collection.Id, secretVariables);
        
        // Update collection with only non-secret variables
        collection.Variables = nonSecretVariables;
        var result = await _collectionRepository.UpdateAsync(collection);
        
        // Restore all variables (including secrets) for the returned object
        foreach (var secret in secretVariables)
        {
            result.Variables[secret.Key] = secret.Value;
        }
        
        return result;
    }

    public async Task DeleteCollectionAsync(Guid id)
    {
        await _secretVariablesService.DeleteCollectionSecretsAsync(id);
        await _collectionRepository.DeleteAsync(id);
    }
}
