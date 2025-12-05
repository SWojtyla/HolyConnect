using System.Text.Json;
using HolyConnect.Application.Interfaces;
using HolyConnect.Domain.Entities;

namespace HolyConnect.Infrastructure.Services;

public class FileBasedSettingsService : ISettingsService
{
    private const string SettingsFileName = "settings.json";
    private readonly string _settingsFilePath;

    public FileBasedSettingsService()
    {
        var appDataPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "HolyConnect");
        Directory.CreateDirectory(appDataPath);
        _settingsFilePath = Path.Combine(appDataPath, SettingsFileName);
    }

    public async Task<AppSettings> GetSettingsAsync()
    {
        if (!File.Exists(_settingsFilePath))
        {
            return GetDefaultSettings();
        }

        try
        {
            var json = await File.ReadAllTextAsync(_settingsFilePath);
            return JsonSerializer.Deserialize<AppSettings>(json) ?? GetDefaultSettings();
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Failed to deserialize settings: {ex.Message}");
            return GetDefaultSettings();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading settings: {ex.Message}");
            return GetDefaultSettings();
        }
    }

    private AppSettings GetDefaultSettings()
    {
        return new AppSettings
        {
            StoragePath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "HolyConnect"),
            IsDarkMode = false
        };
    }

    public async Task SaveSettingsAsync(AppSettings settings)
    {
        var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_settingsFilePath, json);
    }
}
