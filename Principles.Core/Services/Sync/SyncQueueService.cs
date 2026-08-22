using System.Linq.Expressions;
using System.Text.Json;

namespace Principles.Core.Services;

public class SyncQueueService : ISyncQueueService
{
    private static readonly IReadOnlyDictionary<string, int> HandlerOrder = new Dictionary<string, int>
    {
        [nameof( User )] = 0,
        [nameof( UserGoal )] = 1,
        [nameof( UserHabit )] = 2,
        [nameof( ProgressOfHabit )] = 3,
        [nameof( Reminder )] = 4
    };

    private readonly IDatabase m_repository;
    private readonly ISyncRetryConfig m_retryConfig;

    public SyncQueueService( IDatabase repository, ISyncRetryConfig syncRetryConfig )
    {
        m_repository = repository;
        m_retryConfig = syncRetryConfig;
    }

    public async Task AddToQueueAsync( string handlerType, OperationKind operation, IEntity payload )
    {
        await AddToQueueAsync( handlerType, operation, payload, payload.Id, payload.LocalId ).ConfigureAwait( false );
    }

    public async Task AddToQueueAsync( string handlerType, OperationKind operation, object? payload )
    {
        await AddToQueueAsync( handlerType, operation, payload, entityId: null, entityLocalId: null ).ConfigureAwait( false );
    }

    public async Task AddToQueueAsync( string handlerType, OperationKind operation, object? payload, long? entityId, long? entityLocalId )
    {
        SyncQueueItem item = new()
        {
            HandlerType = handlerType,
            Operation = operation.Name,
            PayloadJson = payload is null ? null : JsonSerializer.Serialize( payload, Principles.Constants.Constants.JsonOptions ),
            EntityId = entityId,
            EntityLocalId = entityLocalId,
            NextRetryAt = DateTime.UtcNow
        };

        await m_repository.InsertAsync( item ).ConfigureAwait( false );
    }

    public async Task AddToQueueAsync( string handlerType, OperationKind operation )
    {
        SyncQueueItem item = new()
        {
            HandlerType = handlerType,
            Operation = operation.Name,
            NextRetryAt = DateTime.UtcNow
        };

        await m_repository.InsertAsync( item ).ConfigureAwait( false );
    }

    public Task AddToQueueAsync( IEntity entity, OperationKind operation )
    {
        return AddToQueueAsync( entity.GetType().Name, operation, entity );
    }

    public async Task<List<SyncQueueItem>> GetPendingItemsAsync()
    {
        DateTime now = DateTime.UtcNow;

        return (await m_repository
                .WhereAsync<SyncQueueItem>( item =>
                    !item.IsProcessing &&
                    !item.IsFailed &&
                    !item.IsProcessed &&
                    item.NextRetryAt <= now ).ConfigureAwait( false ))
            .OrderBy( item => GetHandlerOrder( item.HandlerType ) )
            .ThenBy( item => item.LastModified )
            .ThenBy( item => item.LocalId )
            .ToList();
    }

    public Task<List<SyncQueueItem>> GetUnprocessedItemsAsync()
    {
        return GetBlockingItemsAsync();
    }

    public async Task<List<SyncQueueItem>> GetUnprocessedItemsAsync( string handlerType )
    {
        return (await GetBlockingItemsAsync().ConfigureAwait( false ))
            .Where( item => item.HandlerType == handlerType )
            .ToList();
    }

    public Task<List<SyncQueueItem>> GetBlockingItemsAsync()
    {
        return GetOrderedBlockingItemsAsync( item => !item.IsProcessed );
    }

    public async Task<List<SyncQueueItem>> GetBlockingItemsAsync( string handlerType )
    {
        return await GetOrderedBlockingItemsAsync( item => !item.IsProcessed && item.HandlerType == handlerType ).ConfigureAwait( false );
    }

    public async Task MarkAsProcessingAsync( long id )
    {
        SyncQueueItem? item = await m_repository.GetByIdAsync<SyncQueueItem>( id ).ConfigureAwait( false );
        if (item is null)
        {
            return;
        }

        item.IsProcessing = true;
        item.LastRetryAt = DateTime.UtcNow;
        await m_repository.UpdateAsync( item ).ConfigureAwait( false );
    }

    public async Task MarkAsFailedAsync( long id, string errorMessage )
    {
        SyncQueueItem? item = await m_repository.GetByIdAsync<SyncQueueItem>( id ).ConfigureAwait( false );
        if (item is null)
        {
            return;
        }

        item.IsProcessing = false;
        item.ErrorMessage = errorMessage;
        item.RetryCount++;

        if (item.RetryCount >= m_retryConfig.MaxRetryAttempts)
        {
            item.IsFailed = true;
        }
        else
        {
            item.NextRetryAt = CalculateNextRetryTime( item.RetryCount );
        }

        await m_repository.UpdateAsync( item ).ConfigureAwait( false );
    }

    public async Task MarkAsProcessedAsync( long id )
    {
        SyncQueueItem? item = await m_repository.GetByIdAsync<SyncQueueItem>( id ).ConfigureAwait( false );
        if (item is null)
        {
            return;
        }

        item.IsProcessing = false;
        item.IsProcessed = true;
        item.IsFailed = false;
        item.ProcessedAt = DateTime.UtcNow;
        item.ErrorMessage = null;
        await m_repository.UpdateAsync( item ).ConfigureAwait( false );
    }

    public Task RemoveFromQueueAsync( long id )
    {
        return m_repository.DeleteByIdAsync<SyncQueueItem>( id );
    }

    public Task<List<SyncQueueItem>> GetFailedItemsAsync()
    {
        return m_repository.WhereAsync<SyncQueueItem>( item => item.IsFailed );
    }

    public Task<List<SyncQueueItem>> GetStuckItemsAsync()
    {
        DateTime cutoffTime = DateTime.UtcNow.AddMinutes( -30 );
        return m_repository.WhereAsync<SyncQueueItem>( item => item.IsProcessing && item.LastRetryAt < cutoffTime );
    }

    public async Task ResetStuckItemsAsync()
    {
        List<SyncQueueItem> processingItems = await m_repository
            .WhereAsync<SyncQueueItem>( item => item.IsProcessing )
            .ConfigureAwait( false );

        foreach (SyncQueueItem item in processingItems)
        {
            item.IsProcessing = false;
            item.NextRetryAt = DateTime.UtcNow;
            await m_repository.UpdateAsync( item ).ConfigureAwait( false );
        }
    }

    public async Task ResetFailedItemsAsync()
    {
        List<SyncQueueItem> failedItems = await GetFailedItemsAsync().ConfigureAwait( false );
        foreach (SyncQueueItem item in failedItems)
        {
            item.IsFailed = false;
            item.IsProcessed = false;
            item.IsProcessing = false;
            item.NextRetryAt = DateTime.UtcNow;
            await m_repository.UpdateAsync( item ).ConfigureAwait( false );
        }
    }

    public async Task ResetDeferredItemsAsync()
    {
        List<SyncQueueItem> deferredItems = await m_repository
            .WhereAsync<SyncQueueItem>( item => !item.IsProcessed && !item.IsFailed && !item.IsProcessing )
            .ConfigureAwait( false );

        foreach (SyncQueueItem item in deferredItems)
        {
            item.NextRetryAt = DateTime.UtcNow;
            await m_repository.UpdateAsync( item ).ConfigureAwait( false );
        }
    }

    public async Task CompactQueueAsync()
    {
        List<SyncQueueItem> items = await GetBlockingItemsAsync().ConfigureAwait( false );
        if (items.Count == 0)
        {
            return;
        }

        HashSet<long> idsToRemove = [];

        foreach (SyncQueueItem item in items)
        {
            if (await ShouldDropBecauseLocalEntityMissingAsync( item ).ConfigureAwait( false ))
            {
                idsToRemove.Add( item.LocalId );
            }
        }

        List<SyncQueueItem> activeItems = items
            .Where( item => !idsToRemove.Contains( item.LocalId ) )
            .OrderBy( item => item.LastModified )
            .ThenBy( item => item.LocalId )
            .ToList();

        foreach (IGrouping<string, SyncQueueItem> group in activeItems
                     .Where( item => item.HandlerType == nameof( User ) )
                     .GroupBy( item => NormalizeOperationName( item.Operation ) ))
        {
            idsToRemove.UnionWith( group
                .OrderBy( item => item.LastModified )
                .ThenBy( item => item.LocalId )
                .Reverse()
                .Skip( 1 )
                .Select( item => item.LocalId ) );
        }

        foreach (IGrouping<string, SyncQueueItem> group in activeItems
                     .Where( item => item.HandlerType != nameof( User ) && HasEntityKey( item ) )
                     .GroupBy( GetEntityKey ))
        {
            idsToRemove.UnionWith( GetSupersededEntityItemIds( group.ToList() ) );
        }

        foreach (long localId in idsToRemove)
        {
            await m_repository.DeleteByIdAsync<SyncQueueItem>( localId ).ConfigureAwait( false );
        }
    }

    public async Task CleanupOldFailedItemsAsync( TimeSpan ageThreshold )
    {
        DateTime cutoffTime = DateTime.UtcNow.Subtract( ageThreshold );
        List<SyncQueueItem> oldFailedItems = await m_repository
            .WhereAsync<SyncQueueItem>( item => item.IsFailed && item.LastRetryAt < cutoffTime )
            .ConfigureAwait( false );

        foreach (SyncQueueItem item in oldFailedItems)
        {
            await m_repository.DeleteByIdAsync<SyncQueueItem>( item.LocalId ).ConfigureAwait( false );
        }
    }

    public async Task CleanupOldProcessedItemsAsync( TimeSpan ageThreshold )
    {
        DateTime cutoffTime = DateTime.UtcNow.Subtract( ageThreshold );
        List<SyncQueueItem> oldProcessedItems = await m_repository
            .WhereAsync<SyncQueueItem>( item => item.IsProcessed && item.ProcessedAt < cutoffTime )
            .ConfigureAwait( false );

        foreach (SyncQueueItem item in oldProcessedItems)
        {
            await m_repository.DeleteByIdAsync<SyncQueueItem>( item.LocalId ).ConfigureAwait( false );
        }
    }

    private DateTime CalculateNextRetryTime( int retryCount )
    {
        TimeSpan delay = m_retryConfig.BaseDelay;

        for (int retryIndex = 0; retryIndex < retryCount; retryIndex++)
        {
            delay = TimeSpan.FromTicks( (long)(delay.Ticks * m_retryConfig.BackoffMultiplier) );
            if (delay > m_retryConfig.MaxDelay)
            {
                delay = m_retryConfig.MaxDelay;
                break;
            }
        }

        double jitter = Random.Shared.NextDouble() * m_retryConfig.JitterRange.TotalMilliseconds;
        delay = delay.Add( TimeSpan.FromMilliseconds( jitter ) );

        return DateTime.UtcNow.Add( delay );
    }

    private static int GetHandlerOrder( string handlerType )
    {
        return HandlerOrder.TryGetValue( handlerType, out int order ) ? order : int.MaxValue;
    }

    private static bool HasEntityKey( SyncQueueItem item )
    {
        return item.EntityLocalId is long entityLocalId && entityLocalId != 0 ||
            item.EntityId is long entityId && entityId != 0;
    }

    private static string GetEntityKey( SyncQueueItem item )
    {
        if (item.EntityLocalId is long entityLocalId && entityLocalId != 0)
        {
            return $"{item.HandlerType}:local:{entityLocalId}";
        }

        return $"{item.HandlerType}:server:{item.EntityId}";
    }

    private static IEnumerable<long> GetSupersededEntityItemIds( IReadOnlyList<SyncQueueItem> items )
    {
        if (items.Count <= 1)
        {
            return [];
        }

        List<SyncQueueItem> ordered = items
            .OrderBy( item => item.LastModified )
            .ThenBy( item => item.LocalId )
            .ToList();

        SyncQueueItem latestItem = ordered[^1];
        if (IsOperation( latestItem, OperationKind.Delete ) &&
            (latestItem.EntityId is null || latestItem.EntityId == 0))
        {
            return ordered.Select( item => item.LocalId ).ToList();
        }

        if (IsOperation( latestItem, OperationKind.Delete ))
        {
            return ordered
                .Where( item => item.LocalId != latestItem.LocalId )
                .Select( item => item.LocalId )
                .ToList();
        }

        Dictionary<string, long> latestByOperation = ordered
            .GroupBy( item => NormalizeOperationName( item.Operation ) )
            .ToDictionary( group => group.Key, group => group.Last().LocalId );

        return ordered
            .Where( item => latestByOperation[NormalizeOperationName( item.Operation )] != item.LocalId )
            .Select( item => item.LocalId )
            .ToList();
    }

    private async Task<bool> ShouldDropBecauseLocalEntityMissingAsync( SyncQueueItem item )
    {
        if (IsOperation( item, OperationKind.Delete ) ||
            item.EntityLocalId is not long entityLocalId ||
            entityLocalId == 0)
        {
            return false;
        }

        return item.HandlerType switch
        {
            nameof( User ) => await m_repository.GetByIdAsync<User>( entityLocalId ).ConfigureAwait( false ) is null,
            nameof( UserGoal ) => await m_repository.GetByIdAsync<UserGoal>( entityLocalId ).ConfigureAwait( false ) is null,
            nameof( UserHabit ) => await m_repository.GetByIdAsync<UserHabit>( entityLocalId ).ConfigureAwait( false ) is null,
            nameof( ProgressOfHabit ) => await m_repository.GetByIdAsync<ProgressOfHabit>( entityLocalId ).ConfigureAwait( false ) is null,
            nameof( Reminder ) => await m_repository.GetByIdAsync<Reminder>( entityLocalId ).ConfigureAwait( false ) is null,
            _ => false
        };
    }

    private async Task<List<SyncQueueItem>> GetOrderedBlockingItemsAsync( Expression<Func<SyncQueueItem, bool>> predicate )
    {
        return (await m_repository.WhereAsync( predicate ).ConfigureAwait( false ))
            .OrderBy( item => GetHandlerOrder( item.HandlerType ) )
            .ThenBy( item => item.LastModified )
            .ThenBy( item => item.LocalId )
            .ToList();
    }

    private static bool IsOperation( SyncQueueItem item, OperationKind operation )
    {
        return new OperationKind( item.Operation ) == operation;
    }

    private static string NormalizeOperationName( string operation )
    {
        return new OperationKind( operation ).Name;
    }
}
