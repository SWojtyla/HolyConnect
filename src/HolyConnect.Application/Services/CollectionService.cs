using HolyConnect.Application.Common;
using HolyConnect.Application.Interfaces;
using HolyConnect.Domain.Entities;

namespace HolyConnect.Application.Services;

/// <summary>
/// Service for managing collections (independent of environments).
/// </summary>
public class CollectionService : CrudServiceBase<Collection>, ICollectionService
{
    public CollectionService(
        IRepository<Collection> collectionRepository,
        ISecretVariablesService secretVariablesService)
        : base(collectionRepository, secretVariablesService)
    {
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

        return await Repository.AddAsync(collection);
    }

    public async Task<IEnumerable<Collection>> GetAllCollectionsAsync()
    {
        return await GetAllAsync();
    }

    public async Task<Collection?> GetCollectionByIdAsync(Guid id)
    {
        return await GetByIdAsync(id);
    }

    public async Task<Collection> UpdateCollectionAsync(Collection collection)
    {
        return await UpdateAsync(collection);
    }

    public async Task DeleteCollectionAsync(Guid id)
    {
        await DeleteAsync(id);
    }

    public async Task ReorderCollectionsAsync(IEnumerable<Guid> collectionIds)
    {
        var orderedIds = collectionIds.ToList();
        
        // Load all collections in parallel
        var loadTasks = orderedIds.Select(id => GetByIdAsync(id));
        var loadedCollections = await Task.WhenAll(loadTasks);
        
        // Filter out nulls and create list
        var collections = loadedCollections.Where(c => c != null).Cast<Collection>().ToList();
        
        // Assign new order values and collect items that need updating
        var updateTasks = new List<Task<Collection>>();
        for (int i = 0; i < collections.Count; i++)
        {
            if (collections[i].Order != i)
            {
                collections[i].Order = i;
                updateTasks.Add(Repository.UpdateAsync(collections[i]));
            }
        }
        
        // Update all changed items in parallel
        if (updateTasks.Any())
        {
            await Task.WhenAll(updateTasks);
        }
    }

    protected override Guid GetEntityId(Collection entity)
    {
        return entity.Id;
    }

    protected override Dictionary<string, string> GetEntityVariables(Collection entity)
    {
        return entity.Variables;
    }

    protected override void SetEntityVariables(Collection entity, Dictionary<string, string> variables)
    {
        entity.Variables = variables;
    }

    protected override HashSet<string> GetEntitySecretNames(Collection entity)
    {
        return entity.SecretVariableNames;
    }

    protected override async Task LoadAndMergeSecretsAsync(Guid id, Collection entity)
    {
        await SecretVariableHelper.LoadAndMergeSecretsAsync(
            id,
            entity.Variables,
            SecretVariablesService.GetCollectionSecretsAsync);
    }

    protected override async Task SaveSecretsAsync(Guid id, Dictionary<string, string> secrets)
    {
        await SecretVariablesService.SaveCollectionSecretsAsync(id, secrets);
    }

    protected override async Task DeleteSecretsAsync(Guid id)
    {
        await SecretVariablesService.DeleteCollectionSecretsAsync(id);
    }
}
