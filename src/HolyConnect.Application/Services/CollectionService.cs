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

    public async Task<Collection> MoveCollectionAsync(Guid collectionId, Guid? newParentCollectionId)
    {
        var collection = await Repository.GetByIdAsync(collectionId);
        if (collection == null)
        {
            throw new InvalidOperationException($"Collection with ID {collectionId} not found.");
        }

        // Prevent circular references - check if the new parent is a descendant of this collection
        if (newParentCollectionId.HasValue)
        {
            var allCollections = await Repository.GetAllAsync();
            if (IsDescendant(collectionId, newParentCollectionId.Value, allCollections))
            {
                throw new InvalidOperationException("Cannot move a collection into one of its own descendants.");
            }
        }

        // Update the parent collection ID
        collection.ParentCollectionId = newParentCollectionId;
        return await Repository.UpdateAsync(collection);
    }

    private bool IsDescendant(Guid ancestorId, Guid potentialDescendantId, IEnumerable<Collection> allCollections)
    {
        var current = allCollections.FirstOrDefault(c => c.Id == potentialDescendantId);
        while (current != null)
        {
            if (current.ParentCollectionId == ancestorId)
            {
                return true;
            }
            current = allCollections.FirstOrDefault(c => c.Id == current.ParentCollectionId);
        }
        return false;
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
