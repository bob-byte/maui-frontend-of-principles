using Principles.Core.Models;

namespace Principles.Core.Services;

public interface ISyncQueueService
{
    Task AddToQueueAsync(string handlerType, OperationType operation, IEntity entity);
    Task AddToQueueAsync(IEntity entity, OperationType operation);
    Task AddToQueueAsync(string handlerType, string operation, object? payload);
    Task AddToQueueAsync(string handlerType, string operation);
    Task<List<SyncQueueItem>> GetPendingItemsAsync();
    Task MarkAsProcessingAsync(long id);
    Task MarkAsFailedAsync(long id, string errorMessage);
    Task MarkAsProcessedAsync(long id);
    Task RemoveFromQueueAsync(long id);
    Task<List<SyncQueueItem>> GetFailedItemsAsync();
    Task<List<SyncQueueItem>> GetStuckItemsAsync();
    Task ResetStuckItemsAsync();
    Task CleanupOldFailedItemsAsync(TimeSpan ageThreshold);
    Task CleanupOldProcessedItemsAsync(TimeSpan ageThreshold);
}