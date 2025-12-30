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

    public async Task MoveCollectionAsync(Guid collectionId, Guid? newParentCollectionId, int newOrder)
    {
        var collection = await GetByIdAsync(collectionId);
        if (collection == null)
        {
            throw new InvalidOperationException($"Collection with ID {collectionId} not found.");
        }

        var oldParentId = collection.ParentCollectionId;

        // Update the collection's parent and order
        collection.ParentCollectionId = newParentCollectionId;
        collection.Order = newOrder;
        await UpdateAsync(collection);

        // Reorder other collections in the old parent
        if (oldParentId != newParentCollectionId)
        {
            await ReorderCollectionsInParentAsync(oldParentId);
        }

        // Reorder collections in the new parent
        await ReorderCollectionsInParentAsync(newParentCollectionId);
    }

    private async Task ReorderCollectionsInParentAsync(Guid? parentId)
    {
        var allCollections = await GetAllAsync();
        var siblings = allCollections
            .Where(c => c.ParentCollectionId == parentId)
            .OrderBy(c => c.Order)
            .ThenBy(c => c.CreatedAt)
            .ToList();

        for (int i = 0; i < siblings.Count; i++)
        {
            if (siblings[i].Order != i)
            {
                siblings[i].Order = i;
                await UpdateAsync(siblings[i]);
            }
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
