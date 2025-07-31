namespace Principles.Core.Services;

public interface ISyncQueueService
{
    Task AddToQueueAsync(string entityType, string operation, object payload);
    Task<List<SyncQueueItem>> GetPendingItemsAsync();
    Task MarkAsProcessingAsync(int id);
    Task RemoveFromQueueAsync(int id);
}