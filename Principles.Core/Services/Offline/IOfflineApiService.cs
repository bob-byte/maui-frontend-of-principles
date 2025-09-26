namespace Principles.Core.Services;

public interface IOfflineApiService<T> where T : IEntity, new()
{
    Task<List<T>> GetAllAsync(bool forceRefresh = false);
    Task SaveAsync(T item);
    Task ExecuteAsync(Func<Task> localChange, string handlerType, string operation, object? data);
    Task DeleteAsync(T item);
}