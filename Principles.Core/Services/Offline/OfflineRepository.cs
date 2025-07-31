using SQLite;

namespace Principles.Core.Services;

public class OfflineRepository<T> : IOfflineRepository<T> where T : class, IOfflineEntity, new()
{
    private readonly SQLiteConnection _db;
    private readonly ISyncQueueService _syncQueue;

    public OfflineRepository(SQLiteConnection db, ISyncQueueService syncQueue)
    {
        _db = db;
        _db.CreateTable<T>();
        _syncQueue = syncQueue;
    }

    public Task<List<T>> GetAllAsync()
    {
        var result = _db.Table<T>().ToList();
        return Task.FromResult(result);
    }

    public Task<T?> GetByIdAsync(int id)
    {
        var result = _db.Find<T>(id);
        return Task.FromResult(result);
    }

    public Task SaveAsync(T entity)
    {
        
        _db.Insert(entity);

        var op = new SyncQueueItem
        {
            EntityType = typeof(T).Name,
            Operation = "Create",
            PayloadJson = JsonSerializer.Serialize(entity)
        };

        return _syncQueue.EnqueueAsync(op);
    }

    public Task UpdateAsync(T entity)
    {
        _db.Update(entity);

        var op = new SyncQueueItem
        {
            EntityType = typeof(T).Name,
            Operation = "Update",
            PayloadJson = JsonSerializer.Serialize(entity)
        };

        return _syncQueue.EnqueueAsync(op);
    }

    public Task DeleteAsync(int id)
    {
        var entity = _db.Find<T>(id);
        if (entity != null)
        {
            _db.Delete(entity);

            var op = new SyncQueueItem
            {
                EntityType = typeof(T).Name,
                Operation = "Delete",
                PayloadJson = JsonSerializer.Serialize(entity)
            };

            return _syncQueue.EnqueueAsync(op);
        }

        return Task.CompletedTask;
    }
}
