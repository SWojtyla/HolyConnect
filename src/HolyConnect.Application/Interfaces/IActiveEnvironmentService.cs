namespace HolyConnect.Application.Interfaces;

/// <summary>
/// Service for managing the globally active environment.
/// </summary>
public interface IActiveEnvironmentService
{
    /// <summary>
    /// Gets the currently active environment ID.
    /// </summary>
    Task<Guid?> GetActiveEnvironmentIdAsync();
    
    /// <summary>
    /// Sets the active environment ID.
    /// </summary>
    Task SetActiveEnvironmentIdAsync(Guid? environmentId);
    
    /// <summary>
    /// Gets the currently active environment entity.
    /// </summary>
    Task<Domain.Entities.Environment?> GetActiveEnvironmentAsync();
}
