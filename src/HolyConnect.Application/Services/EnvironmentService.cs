using HolyConnect.Application.Interfaces;
using HolyConnect.Domain.Entities;

namespace HolyConnect.Application.Services;

public class EnvironmentService : IEnvironmentService
{
    private readonly IRepository<Domain.Entities.Environment> _environmentRepository;

    public EnvironmentService(IRepository<Domain.Entities.Environment> environmentRepository)
    {
        _environmentRepository = environmentRepository;
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
        return await _environmentRepository.GetByIdAsync(id);
    }

    public async Task<Domain.Entities.Environment> UpdateEnvironmentAsync(Domain.Entities.Environment environment)
    {
        return await _environmentRepository.UpdateAsync(environment);
    }

    public async Task DeleteEnvironmentAsync(Guid id)
    {
        await _environmentRepository.DeleteAsync(id);
    }
}
