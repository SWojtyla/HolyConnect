using HolyConnect.Application.Common;
using HolyConnect.Application.Interfaces;
using HolyConnect.Domain.Entities;

namespace HolyConnect.Application.Services;

public class EnvironmentService : CrudServiceBase<Domain.Entities.Environment>, IEnvironmentService
{
    public EnvironmentService(
        IRepository<Domain.Entities.Environment> environmentRepository,
        ISecretVariablesService secretVariablesService)
        : base(environmentRepository, secretVariablesService)
    {
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

        return await Repository.AddAsync(environment);
    }

    public async Task<IEnumerable<Domain.Entities.Environment>> GetAllEnvironmentsAsync()
    {
        return await GetAllAsync();
    }

    public async Task<Domain.Entities.Environment?> GetEnvironmentByIdAsync(Guid id)
    {
        return await GetByIdAsync(id);
    }

    public async Task<Domain.Entities.Environment> UpdateEnvironmentAsync(Domain.Entities.Environment environment)
    {
        return await UpdateAsync(environment);
    }

    public async Task DeleteEnvironmentAsync(Guid id)
    {
        await DeleteAsync(id);
    }

    protected override Guid GetEntityId(Domain.Entities.Environment entity)
    {
        return entity.Id;
    }

    protected override Dictionary<string, string> GetEntityVariables(Domain.Entities.Environment entity)
    {
        return entity.Variables;
    }

    protected override void SetEntityVariables(Domain.Entities.Environment entity, Dictionary<string, string> variables)
    {
        entity.Variables = variables;
    }

    protected override HashSet<string> GetEntitySecretNames(Domain.Entities.Environment entity)
    {
        return entity.SecretVariableNames;
    }

    protected override async Task LoadAndMergeSecretsAsync(Guid id, Domain.Entities.Environment entity)
    {
        await SecretVariableHelper.LoadAndMergeSecretsAsync(
            id,
            entity.Variables,
            SecretVariablesService.GetEnvironmentSecretsAsync);
    }

    protected override async Task SaveSecretsAsync(Guid id, Dictionary<string, string> secrets)
    {
        await SecretVariablesService.SaveEnvironmentSecretsAsync(id, secrets);
    }

    protected override async Task DeleteSecretsAsync(Guid id)
    {
        await SecretVariablesService.DeleteEnvironmentSecretsAsync(id);
    }
}
