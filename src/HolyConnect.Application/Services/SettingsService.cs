using HolyConnect.Application.Interfaces;
using HolyConnect.Domain.Entities;

namespace HolyConnect.Application.Services;

public class SettingsService
{
    private readonly ISettingsService _settingsProvider;

    public SettingsService(ISettingsService settingsProvider)
    {
        _settingsProvider = settingsProvider;
    }

    public async Task<AppSettings> GetSettingsAsync()
    {
        return await _settingsProvider.GetSettingsAsync();
    }

    public async Task SaveSettingsAsync(AppSettings settings)
    {
        await _settingsProvider.SaveSettingsAsync(settings);
    }
}
