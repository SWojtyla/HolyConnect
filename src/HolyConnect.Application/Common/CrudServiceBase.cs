using HolyConnect.Application.Interfaces;

namespace HolyConnect.Application.Common;

/// <summary>
/// Abstract base class for CRUD services that handle entities with secret variables.
/// Provides common implementations for GetAll, GetById, Update, and Delete operations.
/// </summary>
/// <typeparam name="TEntity">The entity type being managed by this service.</typeparam>
public abstract class CrudServiceBase<TEntity> where TEntity : class
{
    protected readonly IRepository<TEntity> Repository;
    protected readonly ISecretVariablesService SecretVariablesService;

    protected CrudServiceBase(
        IRepository<TEntity> repository,
        ISecretVariablesService secretVariablesService)
    {
        Repository = repository;
        SecretVariablesService = secretVariablesService;
    }

    /// <summary>
    /// Gets all entities from the repository.
    /// </summary>
    public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        return await Repository.GetAllAsync();
    }

    /// <summary>
    /// Gets an entity by ID and loads its secret variables.
    /// </summary>
    /// <param name="id">The entity ID.</param>
    /// <returns>The entity with secrets merged, or null if not found.</returns>
    public virtual async Task<TEntity?> GetByIdAsync(Guid id)
    {
        var entity = await Repository.GetByIdAsync(id);
        if (entity != null)
        {
            await LoadAndMergeSecretsAsync(id, entity);
        }
        return entity;
    }

    /// <summary>
    /// Updates an entity, handling secret variables separately.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    /// <returns>The updated entity with secrets merged.</returns>
    public virtual async Task<TEntity> UpdateAsync(TEntity entity)
    {
        var id = GetEntityId(entity);
        var variables = GetEntityVariables(entity);
        var secretNames = GetEntitySecretNames(entity);

        // Separate secret and non-secret variables
        var separated = SecretVariableHelper.SeparateVariables(variables, secretNames);

        // Save secrets separately
        await SaveSecretsAsync(id, separated.SecretVariables);

        // Update entity with only non-secret variables
        SetEntityVariables(entity, separated.NonSecretVariables);
        var result = await Repository.UpdateAsync(entity);

        // Restore all variables (including secrets) for the returned object
        var resultVariables = GetEntityVariables(result);
        SecretVariableHelper.MergeSecretVariables(resultVariables, separated.SecretVariables);

        return result;
    }

    /// <summary>
    /// Deletes an entity and its associated secrets.
    /// </summary>
    /// <param name="id">The ID of the entity to delete.</param>
    public virtual async Task DeleteAsync(Guid id)
    {
        await DeleteSecretsAsync(id);
        await Repository.DeleteAsync(id);
    }

    /// <summary>
    /// Gets the ID of the entity.
    /// Must be implemented by derived classes.
    /// </summary>
    protected abstract Guid GetEntityId(TEntity entity);

    /// <summary>
    /// Gets the Variables dictionary from the entity.
    /// Must be implemented by derived classes.
    /// </summary>
    protected abstract Dictionary<string, string> GetEntityVariables(TEntity entity);

    /// <summary>
    /// Sets the Variables dictionary on the entity.
    /// Must be implemented by derived classes.
    /// </summary>
    protected abstract void SetEntityVariables(TEntity entity, Dictionary<string, string> variables);

    /// <summary>
    /// Gets the SecretVariableNames set from the entity.
    /// Must be implemented by derived classes.
    /// </summary>
    protected abstract HashSet<string> GetEntitySecretNames(TEntity entity);

    /// <summary>
    /// Loads secrets for the entity and merges them into the entity's variables.
    /// Must be implemented by derived classes to call the appropriate secret service method.
    /// </summary>
    protected abstract Task LoadAndMergeSecretsAsync(Guid id, TEntity entity);

    /// <summary>
    /// Saves secrets for the entity.
    /// Must be implemented by derived classes to call the appropriate secret service method.
    /// </summary>
    protected abstract Task SaveSecretsAsync(Guid id, Dictionary<string, string> secrets);

    /// <summary>
    /// Deletes secrets for the entity.
    /// Must be implemented by derived classes to call the appropriate secret service method.
    /// </summary>
    protected abstract Task DeleteSecretsAsync(Guid id);
}
