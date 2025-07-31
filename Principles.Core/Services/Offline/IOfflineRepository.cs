namespace Principles.Core.Services;

public interface IOfflineRepository<T>
{
    Task<IEnumerable<T>> GetAllAsync();
    Task SaveRangeAsync(IEnumerable<T> items);
    Task SaveAsync(T item);
    Task DeleteAsync(T item);
}