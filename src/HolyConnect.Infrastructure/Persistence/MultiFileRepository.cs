using System.Text.Json;
using HolyConnect.Application.Interfaces;

namespace HolyConnect.Infrastructure.Persistence;

/// <summary>
/// A repository implementation that stores each entity in a separate file for better performance with large collections.
/// </summary>
public class MultiFileRepository<T> : IRepository<T> where T : class
{
    private readonly Func<T, Guid> _idSelector;
    private readonly Func<string> _storagePathProvider;
    private readonly string _directoryName;

    public MultiFileRepository(Func<T, Guid> idSelector, Func<string> storagePathProvider, string directoryName)
    {
        _idSelector = idSelector;
        _storagePathProvider = storagePathProvider;
        _directoryName = directoryName;
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

    private string GetFilePath(Guid id)
    {
        return Path.Combine(GetDirectoryPath(), $"{id}.json");
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
            var options = new JsonSerializerOptions 
            { 
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
            };
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
        var id = _idSelector(entity);
        var filePath = GetFilePath(id);
        
        var options = new JsonSerializerOptions 
        { 
            WriteIndented = true,
            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
        };
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
                var options = new JsonSerializerOptions 
                { 
                    ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
                };
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
        await SaveEntityAsync(entity);
        return entity;
    }

    public Task DeleteAsync(Guid id)
    {
        var filePath = GetFilePath(id);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
        return Task.CompletedTask;
    }
}
