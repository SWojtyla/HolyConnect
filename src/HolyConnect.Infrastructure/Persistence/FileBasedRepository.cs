using System.Text.Json;
using HolyConnect.Application.Interfaces;

namespace HolyConnect.Infrastructure.Persistence;

public class FileBasedRepository<T> : IRepository<T> where T : class
{
    private readonly Func<T, Guid> _idSelector;
    private readonly Func<string> _storagePathProvider;
    private readonly string _fileName;

    public FileBasedRepository(Func<T, Guid> idSelector, Func<string> storagePathProvider, string fileName)
    {
        _idSelector = idSelector;
        _storagePathProvider = storagePathProvider;
        _fileName = fileName;
    }

    private string GetFilePath()
    {
        var storagePath = _storagePathProvider();
        if (string.IsNullOrWhiteSpace(storagePath))
        {
            storagePath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "HolyConnect");
        }
        
        Directory.CreateDirectory(storagePath);
        return Path.Combine(storagePath, _fileName);
    }

    private async Task<Dictionary<Guid, T>> LoadDataAsync()
    {
        var filePath = GetFilePath();
        if (!File.Exists(filePath))
        {
            return new Dictionary<Guid, T>();
        }

        try
        {
            var json = await File.ReadAllTextAsync(filePath);
            var options = new JsonSerializerOptions 
            { 
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
            };
            var list = JsonSerializer.Deserialize<List<T>>(json, options);
            return list?.ToDictionary(_idSelector) ?? new Dictionary<Guid, T>();
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Failed to deserialize data from {filePath}: {ex.Message}");
            return new Dictionary<Guid, T>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading data from {filePath}: {ex.Message}");
            return new Dictionary<Guid, T>();
        }
    }

    private async Task SaveDataAsync(Dictionary<Guid, T> data)
    {
        var filePath = GetFilePath();
        var list = data.Values.ToList();
        var options = new JsonSerializerOptions 
        { 
            WriteIndented = true,
            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
        };
        var json = JsonSerializer.Serialize(list, options);
        await File.WriteAllTextAsync(filePath, json);
    }

    public async Task<T?> GetByIdAsync(Guid id)
    {
        var data = await LoadDataAsync();
        data.TryGetValue(id, out var entity);
        return entity;
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        var data = await LoadDataAsync();
        return data.Values.AsEnumerable();
    }

    public async Task<T> AddAsync(T entity)
    {
        var data = await LoadDataAsync();
        var id = _idSelector(entity);
        data[id] = entity;
        await SaveDataAsync(data);
        return entity;
    }

    public async Task<T> UpdateAsync(T entity)
    {
        var data = await LoadDataAsync();
        var id = _idSelector(entity);
        data[id] = entity;
        await SaveDataAsync(data);
        return entity;
    }

    public async Task DeleteAsync(Guid id)
    {
        var data = await LoadDataAsync();
        data.Remove(id);
        await SaveDataAsync(data);
    }

    public async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities)
    {
        var data = await LoadDataAsync();
        var entityList = entities.ToList();
        
        foreach (var entity in entityList)
        {
            var id = _idSelector(entity);
            data[id] = entity;
        }
        
        await SaveDataAsync(data);
        return entityList;
    }

    public async Task<IEnumerable<T>> UpdateRangeAsync(IEnumerable<T> entities)
    {
        var data = await LoadDataAsync();
        var entityList = entities.ToList();
        
        foreach (var entity in entityList)
        {
            var id = _idSelector(entity);
            data[id] = entity;
        }
        
        await SaveDataAsync(data);
        return entityList;
    }

    public async Task DeleteRangeAsync(IEnumerable<Guid> ids)
    {
        var data = await LoadDataAsync();
        
        foreach (var id in ids)
        {
            data.Remove(id);
        }
        
        await SaveDataAsync(data);
    }
}
