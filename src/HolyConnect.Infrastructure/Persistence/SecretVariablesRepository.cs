using System.Text.Json;
using HolyConnect.Application.Interfaces;

namespace HolyConnect.Infrastructure.Persistence;

/// <summary>
/// Repository for storing secret variable values separately from main entity data.
/// Stores secrets in a separate "secrets" directory which can be git-ignored.
/// </summary>
public class SecretVariablesRepository : ISecretVariablesRepository
{
    private readonly Func<string> _storagePathProvider;
    private const string SecretsDirectoryName = "secrets";

    public SecretVariablesRepository(Func<string> storagePathProvider)
    {
        _storagePathProvider = storagePathProvider;
    }

    private string GetSecretsDirectoryPath()
    {
        var storagePath = _storagePathProvider();
        if (string.IsNullOrWhiteSpace(storagePath))
        {
            storagePath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "HolyConnect");
        }
        
        var secretsPath = Path.Combine(storagePath, SecretsDirectoryName);
        Directory.CreateDirectory(secretsPath);
        return secretsPath;
    }

    private string GetSecretFilePath(string entityType, Guid entityId)
    {
        var secretsDir = GetSecretsDirectoryPath();
        return Path.Combine(secretsDir, $"{entityType}-{entityId}-secrets.json");
    }

    private JsonSerializerOptions GetOptions()
    {
        return new JsonSerializerOptions 
        { 
            WriteIndented = true
        };
    }

    /// <summary>
    /// Gets secret variable values for a specific entity (Environment or Collection).
    /// </summary>
    public async Task<Dictionary<string, string>> GetSecretsAsync(string entityType, Guid entityId)
    {
        var filePath = GetSecretFilePath(entityType, entityId);
        if (!File.Exists(filePath))
        {
            return new Dictionary<string, string>();
        }

        try
        {
            var json = await File.ReadAllTextAsync(filePath);
            var secrets = JsonSerializer.Deserialize<Dictionary<string, string>>(json, GetOptions());
            return secrets ?? new Dictionary<string, string>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading secrets from {filePath}: {ex.Message}");
            return new Dictionary<string, string>();
        }
    }

    /// <summary>
    /// Saves secret variable values for a specific entity (Environment or Collection).
    /// </summary>
    public async Task SaveSecretsAsync(string entityType, Guid entityId, Dictionary<string, string> secrets)
    {
        var filePath = GetSecretFilePath(entityType, entityId);
        
        if (secrets == null || secrets.Count == 0)
        {
            // Delete the file if there are no secrets
            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                }
                catch (IOException ex)
                {
                    Console.WriteLine($"Error deleting secrets file {filePath}: {ex.Message}");
                }
            }
            return;
        }

        var options = GetOptions();
        var json = JsonSerializer.Serialize(secrets, options);
        await File.WriteAllTextAsync(filePath, json);
    }

    /// <summary>
    /// Deletes secret variable values for a specific entity.
    /// </summary>
    public Task DeleteSecretsAsync(string entityType, Guid entityId)
    {
        var filePath = GetSecretFilePath(entityType, entityId);
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Error deleting secrets file {filePath}: {ex.Message}");
        }
        return Task.CompletedTask;
    }
}
