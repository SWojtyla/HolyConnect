using HolyConnect.Application.Interfaces;
using HolyConnect.Domain.Entities;

namespace HolyConnect.Application.Services;

public class EnvironmentService : IEnvironmentService
{
    private readonly IRepository<Domain.Entities.Environment> _environmentRepository;
    private readonly ISecretVariablesService _secretVariablesService;

    public EnvironmentService(
        IRepository<Domain.Entities.Environment> environmentRepository,
        ISecretVariablesService secretVariablesService)
    {
        _environmentRepository = environmentRepository;
        _secretVariablesService = secretVariablesService;
    }

    public async Task<Domain.Entities.Environment> CreateEnvironmentAsync(string name, string? description = null)
    {
        var environment = new Domain.Entities.Environment
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            CreatedAt = DateTime.UtcNow
        };

        return await _environmentRepository.AddAsync(environment);
    }

    public async Task<IEnumerable<Domain.Entities.Environment>> GetAllEnvironmentsAsync()
    {
        return await _environmentRepository.GetAllAsync();
    }

    public async Task<Domain.Entities.Environment?> GetEnvironmentByIdAsync(Guid id)
    {
        var environment = await _environmentRepository.GetByIdAsync(id);
        if (environment != null)
        {
            // Load secret variable values and merge them into the Variables dictionary
            var secrets = await _secretVariablesService.GetEnvironmentSecretsAsync(id);
            foreach (var secret in secrets)
            {
                environment.Variables[secret.Key] = secret.Value;
            }
        }
        return environment;
    }

    public async Task<Domain.Entities.Environment> UpdateEnvironmentAsync(Domain.Entities.Environment environment)
    {
        // Separate secret and non-secret variables
        var secretVariables = new Dictionary<string, string>();
        var nonSecretVariables = new Dictionary<string, string>();
        
        foreach (var variable in environment.Variables)
        {
            if (environment.SecretVariableNames.Contains(variable.Key))
            {
                secretVariables[variable.Key] = variable.Value;
            }
            else
            {
                nonSecretVariables[variable.Key] = variable.Value;
            }
        }
        
        // Save secrets separately
        await _secretVariablesService.SaveEnvironmentSecretsAsync(environment.Id, secretVariables);
        
        // Update environment with only non-secret variables
        environment.Variables = nonSecretVariables;
        var result = await _environmentRepository.UpdateAsync(environment);
        
        // Restore all variables (including secrets) for the returned object
        foreach (var secret in secretVariables)
        {
            result.Variables[secret.Key] = secret.Value;
        }
        
        return result;
    }

    public async Task DeleteEnvironmentAsync(Guid id)
    {
        await _secretVariablesService.DeleteEnvironmentSecretsAsync(id);
        await _environmentRepository.DeleteAsync(id);
    }
}
