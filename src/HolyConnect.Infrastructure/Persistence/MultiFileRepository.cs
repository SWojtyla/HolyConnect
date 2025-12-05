using System.Text.Json;
using HolyConnect.Application.Interfaces;

namespace HolyConnect.Infrastructure.Persistence;

/// <summary>
/// A repository implementation that stores each entity in a separate file for better performance with large collections.
/// File names are readable: "<sanitized-name>__<id>.json" (no legacy support).
/// </summary>
public class MultiFileRepository<T> : IRepository<T> where T : class
{
    private readonly Func<T, Guid> _idSelector;
    private readonly Func<string> _storagePathProvider;
    private readonly string _directoryName;
    private readonly Func<T, string>? _nameSelector;

    public MultiFileRepository(Func<T, Guid> idSelector, Func<string> storagePathProvider, string directoryName, Func<T, string>? nameSelector = null)
    {
        _idSelector = idSelector;
        _storagePathProvider = storagePathProvider;
        _directoryName = directoryName;
        _nameSelector = nameSelector;
    }

    private string GetDirectoryPath()
    {
        var storagePath = _storagePathProvider();
        if (string.IsNullOrWhiteSpace(storagePath))
        {
            storagePath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "HolyConnect");
        }
        
        var directoryPath = Path.Combine(storagePath, _directoryName);
        Directory.CreateDirectory(directoryPath);
        return directoryPath;
    }

    private static string SanitizeFileName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return string.Empty;
        foreach (var c in Path.GetInvalidFileNameChars())
        {
            name = name.Replace(c, '_');
        }
        return name.Trim();
    }

    private string GetReadableFileName(T entity)
    {
        var id = _idSelector(entity);
        var baseName = _nameSelector?.Invoke(entity);
        var readable = SanitizeFileName(baseName ?? string.Empty);
        if (string.IsNullOrWhiteSpace(readable)) readable = id.ToString();
        return $"{readable}__{id}.json";
    }

    private string GetFilePath(Guid id)
    {
        // Search for readable filename pattern only
        var dir = GetDirectoryPath();
        var match = Directory.EnumerateFiles(dir, $"*__{id}.json").FirstOrDefault();
        return match ?? Path.Combine(dir, $"{id}.json"); // deterministic path if name unknown
    }

    private string GetFilePath(T entity)
    {
        var dir = GetDirectoryPath();
        var readable = GetReadableFileName(entity);
        return Path.Combine(dir, readable);
    }

    private JsonSerializerOptions GetOptions()
    {
        var options = new JsonSerializerOptions 
        { 
            WriteIndented = true,
            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
        };
        // Custom polymorphic converter for Request base type if applicable
        if (typeof(T) == typeof(HolyConnect.Domain.Entities.Request) || typeof(T).IsSubclassOf(typeof(HolyConnect.Domain.Entities.Request)))
        {
            options.Converters.Add(new RequestJsonConverter());
        }
        return options;
    }

    private async Task<T?> LoadEntityAsync(Guid id)
    {
        var filePath = GetFilePath(id);
        if (!File.Exists(filePath))
        {
            return null;
        }

        try
        {
            var json = await File.ReadAllTextAsync(filePath);
            var options = GetOptions();
            return JsonSerializer.Deserialize<T>(json, options);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading entity from {filePath}: {ex.Message}");
            return null;
        }
    }

    private async Task SaveEntityAsync(T entity)
    {
        var filePath = GetFilePath(entity);
        var options = GetOptions();
        var json = JsonSerializer.Serialize(entity, options);
        await File.WriteAllTextAsync(filePath, json);
    }

    public async Task<T?> GetByIdAsync(Guid id)
    {
        return await LoadEntityAsync(id);
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        var directoryPath = GetDirectoryPath();
        var files = Directory.GetFiles(directoryPath, "*.json");
        var entities = new List<T>();

        foreach (var file in files)
        {
            try
            {
                var json = await File.ReadAllTextAsync(file);
                var options = GetOptions();
                var entity = JsonSerializer.Deserialize<T>(json, options);
                if (entity != null)
                {
                    entities.Add(entity);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading entity from {file}: {ex.Message}");
            }
        }

        return entities;
    }

    public async Task<T> AddAsync(T entity)
    {
        await SaveEntityAsync(entity);
        return entity;
    }

    public async Task<T> UpdateAsync(T entity)
    {
        // If the readable filename changed (e.g., entity was renamed), delete the old file to avoid duplicates
        var id = _idSelector(entity);
        var oldPath = GetFilePath(id);
        var newPath = GetFilePath(entity);
        if (!string.Equals(oldPath, newPath, StringComparison.OrdinalIgnoreCase) && File.Exists(oldPath))
        {
            try
            {
                File.Delete(oldPath);
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Error deleting old file {oldPath}: {ex.Message}");
            }
        }

        await SaveEntityAsync(entity);
        return entity;
    }

    public Task DeleteAsync(Guid id)
    {
        var filePath = GetFilePath(id);
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Error deleting file {filePath}: {ex.Message}");
        }
        return Task.CompletedTask;
    }
}
