namespace Principles.Core.Services;

public class SyncQueueService : ISyncQueueService
{
    private readonly AppDatabase _database;

    public SyncQueueService(AppDatabase database)
    {
        _database = database;
    }

    public Task AddToQueueAsync(string entityType, string operation, object payload)
    {
        var item = new SyncQueueItem
        {
            EntityType = entityType,
            Operation = operation,
            PayloadJson = JsonSerializer.Serialize(payload)
        };

        _database.Insert(item);
        return Task.CompletedTask;
    }

    public Task<List<SyncQueueItem>> GetPendingItemsAsync()
    {
        var items = _database.SyncQueue
            .Where(i => !i.IsProcessing)
            .OrderBy(i => i.CreatedAt)
            .ToList();

        return Task.FromResult(items);
    }

    public Task MarkAsProcessingAsync(int id)
    {
        var item = _database.Find<SyncQueueItem>(id);
        if (item != null)
        {
            item.IsProcessing = true;
            _database.Update(item);
        }
        return Task.CompletedTask;
    }

    public Task RemoveFromQueueAsync(int id)
    {
        _database.Delete<SyncQueueItem>(id);
        return Task.CompletedTask;
    }
}
