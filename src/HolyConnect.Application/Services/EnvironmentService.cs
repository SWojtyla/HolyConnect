using HolyConnect.Application.Common;
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
            await SecretVariableHelper.LoadAndMergeSecretsAsync(
                id,
                environment.Variables,
                _secretVariablesService.GetEnvironmentSecretsAsync);
        }
        return environment;
    }

    public async Task<Domain.Entities.Environment> UpdateEnvironmentAsync(Domain.Entities.Environment environment)
    {
        // Separate secret and non-secret variables
        var separated = SecretVariableHelper.SeparateVariables(
            environment.Variables,
            environment.SecretVariableNames);
        
        // Save secrets separately
        await _secretVariablesService.SaveEnvironmentSecretsAsync(environment.Id, separated.SecretVariables);
        
        // Update environment with only non-secret variables
        environment.Variables = separated.NonSecretVariables;
        var result = await _environmentRepository.UpdateAsync(environment);
        
        // Restore all variables (including secrets) for the returned object
        SecretVariableHelper.MergeSecretVariables(result.Variables, separated.SecretVariables);
        
        return result;
    }

    public async Task DeleteEnvironmentAsync(Guid id)
    {
        await _secretVariablesService.DeleteEnvironmentSecretsAsync(id);
        await _environmentRepository.DeleteAsync(id);
    }
}
