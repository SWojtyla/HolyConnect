using System.Text.Json;
using HolyConnect.Application.Interfaces;

namespace HolyConnect.Infrastructure.Persistence;

/// <summary>
/// A repository implementation that stores each entity in a separate file for better performance with large collections.
/// Supports hierarchical storage where entities can be organized in subdirectories based on parent relationships.
/// File names use readable names: "{name}.json"
/// When hierarchical path provider is configured, files are stored as: "{base-dir}/{parent-path}/{name}.json"
/// </summary>
public class MultiFileRepository<T> : IRepository<T> where T : class
{
    private readonly Func<T, Guid> _idSelector;
    private readonly Func<string> _storagePathProvider;
    private readonly string _directoryName;
    private readonly Func<T, string>? _nameSelector;
    private readonly Func<T, Task<string>>? _hierarchicalPathProvider;
    private readonly Func<T, Guid?>? _parentIdSelector;

    public MultiFileRepository(
        Func<T, Guid> idSelector, 
        Func<string> storagePathProvider, 
        string directoryName, 
        Func<T, string>? nameSelector = null,
        Func<T, Task<string>>? hierarchicalPathProvider = null,
        Func<T, Guid?>? parentIdSelector = null)
    {
        _idSelector = idSelector;
        _storagePathProvider = storagePathProvider;
        _directoryName = directoryName;
        _nameSelector = nameSelector;
        _hierarchicalPathProvider = hierarchicalPathProvider;
        _parentIdSelector = parentIdSelector;
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

    private async Task<string> GetEntityDirectoryPath(T entity)
    {
        var baseDir = GetDirectoryPath();
        
        // If hierarchical path provider is configured, use subdirectories
        if (_hierarchicalPathProvider != null)
        {
            var hierarchicalPath = await _hierarchicalPathProvider(entity);
            if (!string.IsNullOrWhiteSpace(hierarchicalPath))
            {
                var fullPath = Path.Combine(baseDir, hierarchicalPath);
                Directory.CreateDirectory(fullPath);
                return fullPath;
            }
        }
        
        return baseDir;
    }

    private string GetReadableFileName(T entity)
    {
        var baseName = _nameSelector?.Invoke(entity);
        var readable = SanitizeFileName(baseName ?? string.Empty);
        if (string.IsNullOrWhiteSpace(readable))
        {
            var id = _idSelector(entity);
            readable = id.ToString();
        }
        return $"{readable}.json";
    }

    private async Task<string?> GetFilePath(Guid id)
    {
        // Search for file by loading all entities recursively and finding the one with matching ID
        var baseDir = GetDirectoryPath();
        var files = Directory.GetFiles(baseDir, "*.json", SearchOption.AllDirectories);
        
        foreach (var file in files)
        {
            try
            {
                var json = await File.ReadAllTextAsync(file);
                var options = GetOptions();
                var entity = System.Text.Json.JsonSerializer.Deserialize<T>(json, options);
                if (entity != null && _idSelector(entity) == id)
                {
                    return file;
                }
            }
            catch
            {
                // Skip files that can't be deserialized
            }
        }
        
        return null;
    }

    private async Task<string> GetFilePath(T entity)
    {
        var dir = await GetEntityDirectoryPath(entity);
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
        var filePath = await GetFilePath(id);
        if (filePath == null || !File.Exists(filePath))
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
        var filePath = await GetFilePath(entity);
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
        // Search recursively for all JSON files in subdirectories
        var files = Directory.GetFiles(directoryPath, "*.json", SearchOption.AllDirectories);
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
        // Check for duplicate names if a name selector is provided
        if (_nameSelector != null && _parentIdSelector != null)
        {
            // Check uniqueness within the same parent scope
            var allEntities = await GetAllAsync();
            var entityName = _nameSelector(entity);
            var entityParentId = _parentIdSelector(entity);
            
            var duplicate = allEntities.FirstOrDefault(e => 
                !_idSelector(e).Equals(_idSelector(entity)) &&
                _nameSelector(e).Equals(entityName, StringComparison.OrdinalIgnoreCase) &&
                Equals(_parentIdSelector(e), entityParentId));
            
            if (duplicate != null)
            {
                throw new InvalidOperationException($"An entity with the name '{entityName}' already exists in this scope.");
            }
        }
        else if (_nameSelector != null)
        {
            // Original global uniqueness check
            var newFileName = GetReadableFileName(entity);
            var filePath = await GetFilePath(entity);
            
            if (File.Exists(filePath))
            {
                var existingJson = await File.ReadAllTextAsync(filePath);
                var options = GetOptions();
                var existingEntity = JsonSerializer.Deserialize<T>(existingJson, options);
                
                // Check if the existing file is for a different entity (same name, different ID)
                if (existingEntity != null && !_idSelector(existingEntity).Equals(_idSelector(entity)))
                {
                    var entityName = _nameSelector(entity);
                    throw new InvalidOperationException($"An entity with the name '{entityName}' already exists.");
                }
            }
        }
        
        await SaveEntityAsync(entity);
        return entity;
    }

    public async Task<T> UpdateAsync(T entity)
    {
        var id = _idSelector(entity);
        var oldPath = await GetFilePath(id);
        var newPath = await GetFilePath(entity);
        
        // Check for duplicate names if name selector and parent selector are provided
        if (_nameSelector != null && _parentIdSelector != null)
        {
            // Check uniqueness within the same parent scope
            var allEntities = await GetAllAsync();
            var entityName = _nameSelector(entity);
            var entityParentId = _parentIdSelector(entity);
            
            var duplicate = allEntities.FirstOrDefault(e => 
                !_idSelector(e).Equals(id) &&
                _nameSelector(e).Equals(entityName, StringComparison.OrdinalIgnoreCase) &&
                Equals(_parentIdSelector(e), entityParentId));
            
            if (duplicate != null)
            {
                throw new InvalidOperationException($"An entity with the name '{entityName}' already exists in this scope.");
            }
        }
        else if (_nameSelector != null && oldPath != null && !string.Equals(oldPath, newPath, StringComparison.OrdinalIgnoreCase))
        {
            // Original global uniqueness check
            if (File.Exists(newPath))
            {
                var existingJson = await File.ReadAllTextAsync(newPath);
                var options = GetOptions();
                var existingEntity = JsonSerializer.Deserialize<T>(existingJson, options);
                
                // Check if the existing file is for a different entity (same name, different ID)
                if (existingEntity != null && !_idSelector(existingEntity).Equals(id))
                {
                    var entityName = _nameSelector(entity);
                    throw new InvalidOperationException($"An entity with the name '{entityName}' already exists.");
                }
            }
        }
        
        // If the readable filename or path changed (e.g., entity was renamed or moved), delete the old file to avoid duplicates
        if (oldPath != null && !string.Equals(oldPath, newPath, StringComparison.OrdinalIgnoreCase) && File.Exists(oldPath))
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

    public async Task DeleteAsync(Guid id)
    {
        var filePath = await GetFilePath(id);
        try
        {
            if (filePath != null && File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Error deleting file {filePath}: {ex.Message}");
        }
    }

    public async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities)
    {
        var entityList = entities.ToList();
        var results = new List<T>();

        foreach (var entity in entityList)
        {
            var result = await AddAsync(entity);
            results.Add(result);
        }

        return results;
    }

    public async Task<IEnumerable<T>> UpdateRangeAsync(IEnumerable<T> entities)
    {
        var entityList = entities.ToList();
        var results = new List<T>();

        foreach (var entity in entityList)
        {
            var result = await UpdateAsync(entity);
            results.Add(result);
        }

        return results;
    }

    public async Task DeleteRangeAsync(IEnumerable<Guid> ids)
    {
        var idList = ids.ToList();

        foreach (var id in idList)
        {
            await DeleteAsync(id);
        }
    }
}
