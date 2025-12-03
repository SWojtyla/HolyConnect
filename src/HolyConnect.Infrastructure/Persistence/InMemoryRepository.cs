using HolyConnect.Application.Interfaces;

namespace HolyConnect.Infrastructure.Persistence;

public class InMemoryRepository<T> : IRepository<T> where T : class
{
    private readonly Dictionary<Guid, T> _data = new();
    private readonly Func<T, Guid> _idSelector;

    public InMemoryRepository(Func<T, Guid> idSelector)
    {
        _idSelector = idSelector;
    }

    public Task<T?> GetByIdAsync(Guid id)
    {
        _data.TryGetValue(id, out var entity);
        return Task.FromResult(entity);
    }

    public Task<IEnumerable<T>> GetAllAsync()
    {
        return Task.FromResult(_data.Values.AsEnumerable());
    }

    public Task<T> AddAsync(T entity)
    {
        var id = _idSelector(entity);
        _data[id] = entity;
        return Task.FromResult(entity);
    }

    public Task<T> UpdateAsync(T entity)
    {
        var id = _idSelector(entity);
        _data[id] = entity;
        return Task.FromResult(entity);
    }

    public Task DeleteAsync(Guid id)
    {
        _data.Remove(id);
        return Task.CompletedTask;
    }
}
