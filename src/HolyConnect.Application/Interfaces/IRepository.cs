namespace HolyConnect.Application.Interfaces;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task DeleteAsync(Guid id);
    
    // Batch operations
    Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities);
    Task<IEnumerable<T>> UpdateRangeAsync(IEnumerable<T> entities);
    Task DeleteRangeAsync(IEnumerable<Guid> ids);
}
