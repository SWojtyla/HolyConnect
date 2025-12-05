namespace HolyConnect.Application.Interfaces;

public interface IEnvironmentService
{
    Task<Domain.Entities.Environment> CreateEnvironmentAsync(string name, string? description = null);
    Task<IEnumerable<Domain.Entities.Environment>> GetAllEnvironmentsAsync();
    Task<Domain.Entities.Environment?> GetEnvironmentByIdAsync(Guid id);
    Task<Domain.Entities.Environment> UpdateEnvironmentAsync(Domain.Entities.Environment environment);
    Task DeleteEnvironmentAsync(Guid id);
}
