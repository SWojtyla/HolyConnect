using HolyConnect.Domain.Entities;

namespace HolyConnect.Application.Interfaces;

public interface ISettingsService
{
    Task<AppSettings> GetSettingsAsync();
    Task SaveSettingsAsync(AppSettings settings);
}
