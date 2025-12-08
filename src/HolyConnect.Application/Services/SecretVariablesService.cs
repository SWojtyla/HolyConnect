using HolyConnect.Application.Interfaces;

namespace HolyConnect.Application.Services;

/// <summary>
/// Service implementation for managing secret variables.
/// </summary>
public class SecretVariablesService : ISecretVariablesService
{
    private readonly ISecretVariablesRepository _repository;

    public SecretVariablesService(ISecretVariablesRepository repository)
    {
        _repository = repository;
    }

    public Task<Dictionary<string, string>> GetEnvironmentSecretsAsync(Guid environmentId)
    {
        return _repository.GetSecretsAsync("environment", environmentId);
    }

    public Task SaveEnvironmentSecretsAsync(Guid environmentId, Dictionary<string, string> secrets)
    {
        return _repository.SaveSecretsAsync("environment", environmentId, secrets);
    }

    public Task DeleteEnvironmentSecretsAsync(Guid environmentId)
    {
        return _repository.DeleteSecretsAsync("environment", environmentId);
    }

    public Task<Dictionary<string, string>> GetCollectionSecretsAsync(Guid collectionId)
    {
        return _repository.GetSecretsAsync("collection", collectionId);
    }

    public Task SaveCollectionSecretsAsync(Guid collectionId, Dictionary<string, string> secrets)
    {
        return _repository.SaveSecretsAsync("collection", collectionId, secrets);
    }

    public Task DeleteCollectionSecretsAsync(Guid collectionId)
    {
        return _repository.DeleteSecretsAsync("collection", collectionId);
    }
}
