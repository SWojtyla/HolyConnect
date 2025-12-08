using System.Text.Json;
using HolyConnect.Application.Interfaces;

namespace HolyConnect.Infrastructure.Services;

/// <summary>
/// File-based implementation of secret storage service.
/// Stores secrets in a JSON file separate from the main data files.
/// </summary>
public class FileBasedSecretStorageService : ISecretStorageService
{
    private readonly ISettingsService _settingsService;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private Dictionary<string, Dictionary<string, string>>? _secretsCache;

    public FileBasedSecretStorageService(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    private async Task<string> GetSecretsFilePathAsync()
    {
        var settings = await _settingsService.GetSettingsAsync();
        var secretsPath = settings.SecretsStoragePath;
        
        if (string.IsNullOrWhiteSpace(secretsPath))
        {
            // Use default path if not configured
            var storagePath = !string.IsNullOrWhiteSpace(settings.StoragePath)
                ? settings.StoragePath
                : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "HolyConnect");
            
            secretsPath = storagePath;
        }

        Directory.CreateDirectory(secretsPath);
        return Path.Combine(secretsPath, "secrets.json");
    }

    private async Task<Dictionary<string, Dictionary<string, string>>> LoadSecretsAsync()
    {
        if (_secretsCache != null)
        {
            return _secretsCache;
        }

        await _lock.WaitAsync();
        try
        {
            if (_secretsCache != null)
            {
                return _secretsCache;
            }

            var filePath = await GetSecretsFilePathAsync();
            if (!File.Exists(filePath))
            {
                _secretsCache = new Dictionary<string, Dictionary<string, string>>();
                return _secretsCache;
            }

            var json = await File.ReadAllTextAsync(filePath);
            _secretsCache = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(json) 
                ?? new Dictionary<string, Dictionary<string, string>>();
            
            return _secretsCache;
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task SaveSecretsAsync(Dictionary<string, Dictionary<string, string>> secrets)
    {
        await _lock.WaitAsync();
        try
        {
            var filePath = await GetSecretsFilePathAsync();
            var json = JsonSerializer.Serialize(secrets, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(filePath, json);
            _secretsCache = secrets;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task SetSecretAsync(Guid requestId, string headerKey, string value)
    {
        var secrets = await LoadSecretsAsync();
        var requestKey = requestId.ToString();

        if (!secrets.ContainsKey(requestKey))
        {
            secrets[requestKey] = new Dictionary<string, string>();
        }

        secrets[requestKey][headerKey] = value;
        await SaveSecretsAsync(secrets);
    }

    public async Task<string?> GetSecretAsync(Guid requestId, string headerKey)
    {
        var secrets = await LoadSecretsAsync();
        var requestKey = requestId.ToString();

        if (secrets.TryGetValue(requestKey, out var requestSecrets))
        {
            if (requestSecrets.TryGetValue(headerKey, out var value))
            {
                return value;
            }
        }

        return null;
    }

    public async Task RemoveSecretAsync(Guid requestId, string headerKey)
    {
        var secrets = await LoadSecretsAsync();
        var requestKey = requestId.ToString();

        if (secrets.TryGetValue(requestKey, out var requestSecrets))
        {
            requestSecrets.Remove(headerKey);
            
            // Remove the request entry if no more secrets
            if (requestSecrets.Count == 0)
            {
                secrets.Remove(requestKey);
            }

            await SaveSecretsAsync(secrets);
        }
    }

    public async Task RemoveAllSecretsForRequestAsync(Guid requestId)
    {
        var secrets = await LoadSecretsAsync();
        var requestKey = requestId.ToString();

        if (secrets.Remove(requestKey))
        {
            await SaveSecretsAsync(secrets);
        }
    }
}
