namespace Principles.Core.Services;

public interface IRemoteApiService<T>
{
    Task<List<T>> GetAllAsync(bool forceRefresh = false);
    Task SaveAsync(T item);
    Task DeleteAsync(T item);
    Task ExecuteAsync(string operation, object? data);
}