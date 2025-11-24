using System.Text.Json;

namespace Principles.Core.Services;

public class SyncQueueService : ISyncQueueService
{
    private readonly IDatabase m_repository;
    private readonly ISyncRetryConfig m_retryConfig;

    public SyncQueueService(IDatabase repository, ISyncRetryConfig syncRetryConfig)
    {
        m_repository = repository;
        m_retryConfig = syncRetryConfig;
    }

    public async Task AddToQueueAsync(string handlerType, OperationKind operation, IEntity payload)
    {
        var item = new SyncQueueItem
        {
            HandlerType = handlerType,
            Operation = operation.ToString(),
            PayloadJson = JsonSerializer.Serialize(payload),
            EntityId = payload.Id,
            EntityLocalId = payload.LocalId,
            NextRetryAt = DateTime.UtcNow // Ready to process immediately
        };

        await m_repository.InsertAsync(item);
    }
    
    public async Task AddToQueueAsync(string handlerType, OperationKind operation, object? payload)
    {
        var item = new SyncQueueItem
        {
            HandlerType = handlerType,
            Operation = operation.ToString(),
            PayloadJson = payload is null ? null : JsonSerializer.Serialize(payload),
            NextRetryAt = DateTime.UtcNow // Ready to process immediately
        };

        await m_repository.InsertAsync(item);
    }
    
    public async Task AddToQueueAsync(string handlerType, OperationKind operation)
    {
        var item = new SyncQueueItem
        {
            HandlerType = handlerType,
            Operation = operation.ToString(),
            NextRetryAt = DateTime.UtcNow // Ready to process immediately
        };

        await m_repository.InsertAsync(item);
    }

    public Task AddToQueueAsync(IEntity entity, OperationKind operation)
    {
        return AddToQueueAsync(entity.GetType().Name, operation, entity);
    }

    public async Task<List<SyncQueueItem>> GetPendingItemsAsync()
    {
        var now = DateTime.UtcNow;
        List<SyncQueueItem> items = (await m_repository
            .WhereAsync<SyncQueueItem>(i => 
                !i.IsProcessing && 
                !i.IsFailed && 
                !i.IsProcessed && 
                i.NextRetryAt <= now))
            .OrderBy(i => i.NextRetryAt)
            .ThenBy(i => i.LastModified)
            .ToList();

        return items;
    }

    public async Task MarkAsProcessingAsync(long id)
    {
        SyncQueueItem? item = await m_repository.GetByIdAsync<SyncQueueItem>(id);
        if (item != null)
        {
            item.IsProcessing = true;
            item.LastRetryAt = DateTime.UtcNow;
            await m_repository.UpdateAsync(item);
        }
    }

    public async Task MarkAsFailedAsync(long id, string errorMessage)
    {
        SyncQueueItem? item = await m_repository.GetByIdAsync<SyncQueueItem>(id);
        if (item != null)
        {
            item.IsProcessing = false;
            item.ErrorMessage = errorMessage;
            item.RetryCount++;
            
            if (item.RetryCount >= m_retryConfig.MaxRetryAttempts)
            {
                item.IsFailed = true;
                item.IsProcessed = true;
            }
            else
            {
                item.NextRetryAt = CalculateNextRetryTime(item.RetryCount);
            }
            
            await m_repository.UpdateAsync(item);
        }
    }

    public async Task MarkAsProcessedAsync(long id)
    {
        SyncQueueItem? item = await m_repository.GetByIdAsync<SyncQueueItem>(id);
        if (item != null)
        {
            item.IsProcessing = false;
            item.IsProcessed = true;
            item.ProcessedAt = DateTime.UtcNow;
            item.ErrorMessage = null; // Clear any previous errors
            await m_repository.UpdateAsync(item);
        }
    }

    public async Task RemoveFromQueueAsync(long id)
    {
        await m_repository.DeleteByIdAsync<SyncQueueItem>(id);
    }

    public async Task<List<SyncQueueItem>> GetFailedItemsAsync()
    {
        return await m_repository.WhereAsync<SyncQueueItem>(i => i.IsFailed);
    }

    public async Task<List<SyncQueueItem>> GetStuckItemsAsync()
    {
        var cutoffTime = DateTime.UtcNow.AddMinutes(-30); // Items stuck for more than 30 minutes
        return await m_repository.WhereAsync<SyncQueueItem>(i => 
            i.IsProcessing && i.LastRetryAt < cutoffTime);
    }

    public async Task ResetStuckItemsAsync()
    {
        List<SyncQueueItem> stuckItems = await GetStuckItemsAsync();
        foreach (SyncQueueItem item in stuckItems)
        {
            item.IsProcessing = false;
            item.NextRetryAt = DateTime.UtcNow; // Retry immediately
            await m_repository.UpdateAsync(item);
        }
    }

    public async Task CleanupOldFailedItemsAsync(TimeSpan ageThreshold)
    {
        var cutoffTime = DateTime.UtcNow.Subtract(ageThreshold);
        var oldFailedItems = await m_repository.WhereAsync<SyncQueueItem>(i => 
            i.IsFailed && i.LastRetryAt < cutoffTime);
        
        foreach (var item in oldFailedItems)
        {
            await m_repository.DeleteByIdAsync<SyncQueueItem>(item.LocalId);
        }
    }

    public async Task CleanupOldProcessedItemsAsync(TimeSpan ageThreshold)
    {
        var cutoffTime = DateTime.UtcNow.Subtract(ageThreshold);
        var oldProcessedItems = await m_repository.WhereAsync<SyncQueueItem>(i => 
            i.IsProcessed && i.ProcessedAt < cutoffTime);
        
        foreach (var item in oldProcessedItems)
        {
            await m_repository.DeleteByIdAsync<SyncQueueItem>(item.LocalId);
        }
    }

    private DateTime CalculateNextRetryTime(int retryCount)
    {
        var delay = m_retryConfig.BaseDelay;
        
        // Exponential backoff
        for (int i = 0; i < retryCount; i++)
        {
            delay = TimeSpan.FromTicks((long)(delay.Ticks * m_retryConfig.BackoffMultiplier));
            if (delay > m_retryConfig.MaxDelay)
            {
                delay = m_retryConfig.MaxDelay;
                break;
            }
        }
        
        // Add jitter to prevent thundering herd
        var random = new Random();
        var jitter = random.NextDouble() * m_retryConfig.JitterRange.TotalMilliseconds;
        delay = delay.Add(TimeSpan.FromMilliseconds(jitter));
        
        return DateTime.UtcNow.Add(delay);
    }
}
