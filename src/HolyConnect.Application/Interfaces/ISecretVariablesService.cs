namespace HolyConnect.Application.Interfaces;

/// <summary>
/// Repository interface for storing secret variable values separately from main entity data.
/// </summary>
public interface ISecretVariablesRepository
{
    /// <summary>
    /// Gets secret variable values for a specific entity (Environment or Collection).
    /// </summary>
    Task<Dictionary<string, string>> GetSecretsAsync(string entityType, Guid entityId);

    /// <summary>
    /// Saves secret variable values for a specific entity (Environment or Collection).
    /// </summary>
    Task SaveSecretsAsync(string entityType, Guid entityId, Dictionary<string, string> secrets);

    /// <summary>
    /// Deletes secret variable values for a specific entity.
    /// </summary>
    Task DeleteSecretsAsync(string entityType, Guid entityId);
}

/// <summary>
/// Service for managing secret variables for environments and collections.
/// Secret variables are stored separately and excluded from git.
/// </summary>
public interface ISecretVariablesService
{
    /// <summary>
    /// Gets secret variable values for an environment.
    /// </summary>
    Task<Dictionary<string, string>> GetEnvironmentSecretsAsync(Guid environmentId);

    /// <summary>
    /// Saves secret variable values for an environment.
    /// </summary>
    Task SaveEnvironmentSecretsAsync(Guid environmentId, Dictionary<string, string> secrets);

    /// <summary>
    /// Deletes secret variable values for an environment.
    /// </summary>
    Task DeleteEnvironmentSecretsAsync(Guid environmentId);

    /// <summary>
    /// Gets secret variable values for a collection.
    /// </summary>
    Task<Dictionary<string, string>> GetCollectionSecretsAsync(Guid collectionId);

    /// <summary>
    /// Saves secret variable values for a collection.
    /// </summary>
    Task SaveCollectionSecretsAsync(Guid collectionId, Dictionary<string, string> secrets);

    /// <summary>
    /// Deletes secret variable values for a collection.
    /// </summary>
    Task DeleteCollectionSecretsAsync(Guid collectionId);
}
