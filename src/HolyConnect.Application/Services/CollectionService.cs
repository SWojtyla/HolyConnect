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

    public async Task UpdateCollectionOrderAsync(IEnumerable<(Guid CollectionId, int OrderIndex)> collectionOrders)
    {
        foreach (var (collectionId, orderIndex) in collectionOrders)
        {
            var collection = await Repository.GetByIdAsync(collectionId);
            if (collection != null)
            {
                collection.OrderIndex = orderIndex;
                await Repository.UpdateAsync(collection);
            }
        }
    }

    public async Task MoveCollectionAsync(Guid collectionId, bool moveUp)
    {
        var collection = await Repository.GetByIdAsync(collectionId);
        if (collection == null)
        {
            throw new InvalidOperationException($"Collection with ID {collectionId} not found.");
        }

        // Get all collections with the same parent
        var allCollections = await Repository.GetAllAsync();
        var siblings = allCollections
            .Where(c => c.ParentCollectionId == collection.ParentCollectionId)
            .OrderBy(c => c.OrderIndex)
            .ThenBy(c => c.CreatedAt)
            .ToList();

        var currentIndex = siblings.FindIndex(c => c.Id == collectionId);
        if (currentIndex == -1) return;

        // Determine target index based on direction
        int targetIndex = moveUp ? currentIndex - 1 : currentIndex + 1;

        // Check bounds
        if (targetIndex < 0 || targetIndex >= siblings.Count) return;

        // Swap OrderIndex values
        var targetCollection = siblings[targetIndex];
        var tempOrderIndex = collection.OrderIndex;
        collection.OrderIndex = targetCollection.OrderIndex;
        targetCollection.OrderIndex = tempOrderIndex;

        await Repository.UpdateAsync(collection);
        await Repository.UpdateAsync(targetCollection);
    }
}
