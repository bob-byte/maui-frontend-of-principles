using Principles.Core.Models;

namespace Principles.Core.Services;

public interface ISyncQueueService
{
    Task AddToQueueAsync(string handlerType, OperationKind operation, IEntity entity);
    Task AddToQueueAsync(IEntity entity, OperationKind operation);
    Task AddToQueueAsync(string handlerType, OperationKind operation, object? payload);
    Task AddToQueueAsync(string handlerType, OperationKind operation, object? payload, long? entityId, long? entityLocalId);
    Task AddToQueueAsync(string handlerType, OperationKind operation);
    Task<List<SyncQueueItem>> GetPendingItemsAsync();
    Task<List<SyncQueueItem>> GetUnprocessedItemsAsync();
    Task<List<SyncQueueItem>> GetUnprocessedItemsAsync( string handlerType );
    Task<List<SyncQueueItem>> GetBlockingItemsAsync();
    Task<List<SyncQueueItem>> GetBlockingItemsAsync( string handlerType );
    Task MarkAsProcessingAsync(long id);
    Task MarkAsFailedAsync(long id, string errorMessage);
    Task MarkAsProcessedAsync(long id);
    Task RemoveFromQueueAsync(long id);
    Task<List<SyncQueueItem>> GetFailedItemsAsync();
    Task<List<SyncQueueItem>> GetStuckItemsAsync();
    Task ResetStuckItemsAsync();
    Task ResetFailedItemsAsync();
    Task ResetDeferredItemsAsync();
    Task CompactQueueAsync();
    Task CleanupOldFailedItemsAsync(TimeSpan ageThreshold);
    Task CleanupOldProcessedItemsAsync(TimeSpan ageThreshold);
}
