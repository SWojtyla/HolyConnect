using HolyConnect.Application.Interfaces;

namespace HolyConnect.Application.Services;

/// <summary>
/// Service for managing the globally active environment.
/// </summary>
public class ActiveEnvironmentService : IActiveEnvironmentService
{
    private readonly ISettingsService _settingsService;
    private readonly IRepository<Domain.Entities.Environment> _environmentRepository;

    public ActiveEnvironmentService(
        ISettingsService settingsService,
        IRepository<Domain.Entities.Environment> environmentRepository)
    {
        _settingsService = settingsService;
        _environmentRepository = environmentRepository;
    }

    public async Task<Guid?> GetActiveEnvironmentIdAsync()
    {
        var settings = await _settingsService.GetSettingsAsync();
        return settings.ActiveEnvironmentId;
    }

    public async Task SetActiveEnvironmentIdAsync(Guid? environmentId)
    {
        var settings = await _settingsService.GetSettingsAsync();
        settings.ActiveEnvironmentId = environmentId;
        await _settingsService.SaveSettingsAsync(settings);
    }

    public async Task<Domain.Entities.Environment?> GetActiveEnvironmentAsync()
    {
        var activeEnvId = await GetActiveEnvironmentIdAsync();
        if (!activeEnvId.HasValue)
        {
            return null;
        }

        return await _environmentRepository.GetByIdAsync(activeEnvId.Value);
    }
}
