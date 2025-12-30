using System.Text.Json;
using HolyConnect.Application.Interfaces;
using HolyConnect.Domain.Entities;

namespace HolyConnect.Infrastructure.Services;

public class FileBasedSettingsService : ISettingsService
{
    private const string SettingsFileName = "settings.json";
    private const string BackupSettingsFileName = "settings.backup.json";
    private readonly string _settingsFilePath;
    private readonly string _backupSettingsFilePath;
    private readonly string _appDataPath;

    public FileBasedSettingsService()
    {
        _appDataPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "HolyConnect");
        Directory.CreateDirectory(_appDataPath);
        _settingsFilePath = Path.Combine(_appDataPath, SettingsFileName);
        _backupSettingsFilePath = Path.Combine(_appDataPath, BackupSettingsFileName);
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
            var settings = JsonSerializer.Deserialize<AppSettings>(json) ?? GetDefaultSettings();
            
            // Ensure GitFolders is never null (for backward compatibility with old settings files)
            if (settings.GitFolders == null)
            {
                settings.GitFolders = new List<GitFolder>();
            }
            
            // Ensure EnvironmentOrder is never null (for backward compatibility with old settings files)
            if (settings.EnvironmentOrder == null)
            {
                settings.EnvironmentOrder = new List<Guid>();
            }
            
            return settings;
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Failed to deserialize settings: {ex.Message}");
            // Attempt to restore from backup
            return await TryRestoreFromBackupAsync() ?? GetDefaultSettings();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading settings: {ex.Message}");
            // Attempt to restore from backup
            return await TryRestoreFromBackupAsync() ?? GetDefaultSettings();
        }
    }

    private async Task<AppSettings?> TryRestoreFromBackupAsync()
    {
        if (!File.Exists(_backupSettingsFilePath))
        {
            Console.WriteLine("No backup settings file found");
            return null;
        }

        try
        {
            Console.WriteLine("Attempting to restore settings from backup...");
            var json = await File.ReadAllTextAsync(_backupSettingsFilePath);
            var settings = JsonSerializer.Deserialize<AppSettings>(json);
            
            if (settings != null)
            {
                // Ensure GitFolders is never null
                if (settings.GitFolders == null)
                {
                    settings.GitFolders = new List<GitFolder>();
                }
                
                // Ensure EnvironmentOrder is never null
                if (settings.EnvironmentOrder == null)
                {
                    settings.EnvironmentOrder = new List<Guid>();
                }
                
                Console.WriteLine($"Successfully restored settings from backup with {settings.GitFolders.Count} git repositories");
                
                // Restore the main settings file from backup atomically
                await WriteFileAtomicallyAsync(_settingsFilePath, json);
                
                return settings;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to restore from backup: {ex.Message}");
        }

        return null;
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
        
        // Create backup of existing settings before saving new ones
        if (File.Exists(_settingsFilePath))
        {
            try
            {
                // Use atomic copy via temp file with unique name to avoid conflicts
                var tempBackupPath = Path.Combine(_appDataPath, $"{BackupSettingsFileName}.{Guid.NewGuid():N}.tmp");
                File.Copy(_settingsFilePath, tempBackupPath, overwrite: true);
                File.Move(tempBackupPath, _backupSettingsFilePath, overwrite: true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to create settings backup: {ex.Message}");
                // Continue with save even if backup fails
            }
        }
        
        // Write settings atomically
        await WriteFileAtomicallyAsync(_settingsFilePath, json);
    }

    private async Task WriteFileAtomicallyAsync(string filePath, string content)
    {
        // Write to a temporary file first with unique name to avoid conflicts
        var tempPath = Path.Combine(_appDataPath, $"{Path.GetFileName(filePath)}.{Guid.NewGuid():N}.tmp");
        
        try
        {
            await File.WriteAllTextAsync(tempPath, content);
            
            // Atomic rename to replace the original file
            File.Move(tempPath, filePath, overwrite: true);
        }
        catch
        {
            // Clean up temp file if something went wrong
            if (File.Exists(tempPath))
            {
                try 
                { 
                    File.Delete(tempPath); 
                } 
                catch 
                { 
                    // Intentionally ignore cleanup failures to avoid masking the original exception
                    // The temp file will be cleaned up by the OS eventually
                }
            }
            throw;
        }
    }
}
