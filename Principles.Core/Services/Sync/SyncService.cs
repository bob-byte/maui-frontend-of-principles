using Principles.Core.Models;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;
using System.Net;

namespace Principles.Core.Services;

public class SyncService : ISyncService
{
    private readonly List<ISyncQueueHandler> m_handlers;
    private readonly ISyncQueueService m_queue;
    private readonly ILoggingService m_loggingService;
    private readonly ISettingsService m_settingsService;
    private readonly ISyncSnapshotRemoteApi m_snapshotRemoteApi;
    private readonly ISyncSnapshotMergeService m_snapshotMergeService;
    private readonly IDatabase m_database;
    private int m_isSyncing;

    public SyncService(IServiceProvider serviceProvider)
    {
        m_queue = serviceProvider.GetRequiredService<ISyncQueueService>();
        m_loggingService = serviceProvider.GetRequiredService<ILoggingService>();
        m_settingsService = serviceProvider.GetRequiredService<ISettingsService>();
        m_snapshotRemoteApi = serviceProvider.GetRequiredService<ISyncSnapshotRemoteApi>();
        m_snapshotMergeService = serviceProvider.GetRequiredService<ISyncSnapshotMergeService>();
        m_database = serviceProvider.GetRequiredService<IDatabase>();

        m_handlers =
        [
            serviceProvider.GetRequiredService<RemoteApiService<User>>(),
            serviceProvider.GetRequiredService<RemoteApiService<UserGoal>>(),
            serviceProvider.GetRequiredService<RemoteApiService<UserHabit>>(),
            serviceProvider.GetRequiredService<RemoteApiService<ProgressOfHabit>>(),
            serviceProvider.GetRequiredService<RemoteApiService<Reminder>>()
        ];
    }

    public async Task SyncAsync()
    {
        string authToken = m_settingsService.AuthAccessToken ?? await m_settingsService.GetAuthAccessTokenAsync().ConfigureAwait( false );
        if (string.IsNullOrWhiteSpace( authToken ))
        {
            return;
        }

        if (Interlocked.Exchange( ref m_isSyncing, 1 ) == 1)
        {
            LogInfo( "Sync is already in progress." );
            return;
        }

        try
        {
            await NormalizeLegacyQueueOperationsAsync().ConfigureAwait( false );
            await m_queue.ResetFailedItemsAsync().ConfigureAwait( false );
            await m_queue.ResetStuckItemsAsync().ConfigureAwait( false );
            await m_queue.ResetDeferredItemsAsync().ConfigureAwait( false );
            await m_queue.CompactQueueAsync().ConfigureAwait( false );

            LogInfo( "Pulling initial sync bootstrap snapshot." );
            SyncBootstrapResponse initialSnapshot = await m_snapshotRemoteApi.GetBootstrapAsync().ConfigureAwait( false );
            LogBootstrap( "Initial bootstrap received", initialSnapshot );

            await ReconcileQueueAgainstSnapshotAsync( initialSnapshot ).ConfigureAwait( false );
            await m_snapshotMergeService.MergeAsync( initialSnapshot ).ConfigureAwait( false );

            List<SyncQueueItem> initialBlockingItems = await m_queue.GetBlockingItemsAsync().ConfigureAwait( false );
            LogInfo( $"Queue reconciliation finished. Blocking items: {initialBlockingItems.Count}" );

            while (true)
            {
                List<SyncQueueItem> items = await m_queue.GetPendingItemsAsync().ConfigureAwait( false );
                if (items.Count == 0)
                {
                    break;
                }

                LogInfo($"Processing {items.Count} sync items");

                int processedItems = 0;
                foreach (SyncQueueItem item in items)
                {
                    if (await ProcessQueueItemAsync( item ).ConfigureAwait( false ))
                    {
                        processedItems++;
                    }
                }

                if (processedItems == 0)
                {
                    LogInfo( "Sync queue processing is blocked by failed items." );
                    break;
                }

                await m_queue.CompactQueueAsync().ConfigureAwait( false );
            }

            await m_queue.CompactQueueAsync().ConfigureAwait( false );
            List<SyncQueueItem> remainingBlockingItems = await m_queue.GetBlockingItemsAsync().ConfigureAwait( false );
            LogInfo( $"Queue processing finished. Remaining blocking items: {remainingBlockingItems.Count}" );

            LogInfo( "Pulling final sync bootstrap snapshot." );
            SyncBootstrapResponse snapshot = await m_snapshotRemoteApi.GetBootstrapAsync().ConfigureAwait( false );
            LogBootstrap( "Final bootstrap received", snapshot );
            await m_snapshotMergeService.MergeAsync( snapshot ).ConfigureAwait( false );
            LogInfo( "Final bootstrap merge finished." );
        }
        catch (Exception ex)
        {
            m_loggingService.LogError(ex, "Error during sync process");
            throw;
        }
        finally
        {
            Interlocked.Exchange( ref m_isSyncing, 0 );
        }
    }

    private void LogInfo( string message )
    {
        if (m_settingsService.IsDebug)
        {
            m_loggingService.LogInfo(message);
        }
    }

    private void LogBootstrap( string message, SyncBootstrapResponse snapshot )
    {
        LogInfo(
            $"{message}. Goals: {snapshot.Goals?.Count ?? 0}, " +
            $"Active habits: {snapshot.ActiveHabits?.Count ?? 0}, " +
            $"Archived habits: {snapshot.ArchivedHabits?.Count ?? 0}, " +
            $"Has user: {snapshot.User is not null}, " +
            $"Has reminder: {snapshot.HabitsReportReminder is not null}" );
    }

    private async Task NormalizeLegacyQueueOperationsAsync()
    {
        List<SyncQueueItem> queueItems = await m_database.GetAllAsync<SyncQueueItem>().ConfigureAwait( false );
        foreach (SyncQueueItem item in queueItems)
        {
            OperationKind operation = new( item.Operation );
            if (item.Operation == operation.Name)
            {
                continue;
            }

            item.Operation = operation.Name;

            if (item.HandlerType == nameof( UserHabit ) && item.IsProcessed)
            {
                item.IsProcessed = false;
                item.IsFailed = false;
                item.IsProcessing = false;
                item.ProcessedAt = null;
                item.NextRetryAt = DateTime.UtcNow;
                item.ErrorMessage = null;
            }

            await m_database.UpdateAsync( item ).ConfigureAwait( false );
        }
    }

    private async Task ReconcileQueueAgainstSnapshotAsync( SyncBootstrapResponse snapshot )
    {
        List<SyncQueueItem> queueItems = await m_queue.GetBlockingItemsAsync().ConfigureAwait( false );
        if (queueItems.Count == 0)
        {
            return;
        }

        Dictionary<long, UserGoal> serverGoalsById = (snapshot.Goals ?? [])
            .Where( goal => goal.Id != 0 )
            .ToDictionary( goal => goal.Id );

        List<UserHabit> serverHabits = (snapshot.ActiveHabits ?? [])
            .Concat( snapshot.ArchivedHabits ?? [] )
            .ToList();
        Dictionary<long, UserHabit> serverHabitsById = serverHabits
            .Where( habit => habit.Id != 0 )
            .ToDictionary( habit => habit.Id );
        Dictionary<long, ProgressOfHabit> serverProgressesById = serverHabits
            .SelectMany( habit => habit.Progresses ?? [] )
            .Where( progress => progress.Id != 0 )
            .GroupBy( progress => progress.Id )
            .ToDictionary( group => group.Key, group => group.First() );

        foreach (SyncQueueItem item in queueItems)
        {
            DateTime? serverTimestamp = await GetServerTimestampAsync(
                item,
                snapshot,
                serverGoalsById,
                serverHabitsById,
                serverProgressesById
            ).ConfigureAwait( false );

            if (serverTimestamp is null)
            {
                continue;
            }

            DateTime localTimestamp = await GetLocalTimestampAsync( item ).ConfigureAwait( false );
            if (localTimestamp == default)
            {
                localTimestamp = item.LastModified;
            }

            if (NormalizeTimestamp( serverTimestamp.Value ) >= NormalizeTimestamp( localTimestamp ))
            {
                LogInfo( $"Queue item {item.LocalId} is older than server state and will be skipped." );
                await m_queue.MarkAsProcessedAsync( item.LocalId ).ConfigureAwait( false );
            }
        }
    }

    private async Task<DateTime?> GetServerTimestampAsync(
        SyncQueueItem item,
        SyncBootstrapResponse snapshot,
        IReadOnlyDictionary<long, UserGoal> serverGoalsById,
        IReadOnlyDictionary<long, UserHabit> serverHabitsById,
        IReadOnlyDictionary<long, ProgressOfHabit> serverProgressesById )
    {
        if (item.HandlerType == nameof( User ))
        {
            return snapshot.User?.LastModified;
        }

        if (item.HandlerType == nameof( Reminder ))
        {
            return snapshot.HabitsReportReminder?.LastModified;
        }

        long entityId = await GetEntityServerIdAsync( item ).ConfigureAwait( false );
        if (entityId == 0)
        {
            return null;
        }

        if (item.HandlerType == nameof( UserGoal ) && serverGoalsById.TryGetValue( entityId, out UserGoal? goal ))
        {
            return goal.LastModified;
        }

        if (item.HandlerType == nameof( UserHabit ) && serverHabitsById.TryGetValue( entityId, out UserHabit? habit ))
        {
            return habit.LastModified;
        }

        if (item.HandlerType == nameof( ProgressOfHabit ) && serverProgressesById.TryGetValue( entityId, out ProgressOfHabit? progress ))
        {
            return progress.LastModified;
        }

        return null;
    }

    private async Task<long> GetEntityServerIdAsync( SyncQueueItem item )
    {
        if (item.EntityId is long entityId && entityId != 0)
        {
            return entityId;
        }

        if (item.EntityLocalId is not long localId || localId == 0)
        {
            return 0;
        }

        return item.HandlerType switch
        {
            nameof( UserGoal ) => await m_database.GetFieldAsync<UserGoal, long>( localId, nameof( UserGoal.Id ) ).ConfigureAwait( false ),
            nameof( UserHabit ) => await m_database.GetFieldAsync<UserHabit, long>( localId, nameof( UserHabit.Id ) ).ConfigureAwait( false ),
            nameof( ProgressOfHabit ) => await m_database.GetFieldAsync<ProgressOfHabit, long>( localId, nameof( ProgressOfHabit.Id ) ).ConfigureAwait( false ),
            _ => 0
        };
    }

    private async Task<DateTime> GetLocalTimestampAsync( SyncQueueItem item )
    {
        if (item.EntityLocalId is not long localId || localId == 0)
        {
            return item.LastModified;
        }

        return item.HandlerType switch
        {
            nameof( User ) => await m_database.GetFieldAsync<User, DateTime>( localId, nameof( User.LastModified ) ).ConfigureAwait( false ),
            nameof( UserGoal ) => await m_database.GetFieldAsync<UserGoal, DateTime>( localId, nameof( UserGoal.LastModified ) ).ConfigureAwait( false ),
            nameof( UserHabit ) => await m_database.GetFieldAsync<UserHabit, DateTime>( localId, nameof( UserHabit.LastModified ) ).ConfigureAwait( false ),
            nameof( ProgressOfHabit ) => await m_database.GetFieldAsync<ProgressOfHabit, DateTime>( localId, nameof( ProgressOfHabit.LastModified ) ).ConfigureAwait( false ),
            nameof( Reminder ) => await m_database.GetFieldAsync<Reminder, DateTime>( localId, nameof( Reminder.LastModified ) ).ConfigureAwait( false ),
            _ => item.LastModified
        };
    }

    private static DateTime NormalizeTimestamp( DateTime timestamp )
    {
        return timestamp == default ? DateTime.MinValue : DateTime.SpecifyKind( timestamp, DateTimeKind.Utc );
    }

    private async Task<bool> ProcessQueueItemAsync(SyncQueueItem item)
    {
        try
        {
            await m_queue.MarkAsProcessingAsync(item.LocalId);

            LogInfo($"Processing sync item {item.LocalId}: {item.HandlerType}.{item.Operation}");

            ISyncQueueHandler? handler = m_handlers.FirstOrDefault(h => h.CanHandle(item.HandlerType));
            if (handler is null)
            {
                string errorMessage = $"Handler for entity \"{item.HandlerType}\" not found";
                m_loggingService.LogFatal( errorMessage );
                IDialogService dialogService = ServiceLocator.Current!.GetRequiredService<IDialogService>();
                await dialogService.ShowErrorAsync( errorMessage );
                await m_queue.MarkAsFailedAsync(item.LocalId, errorMessage);
                return false;
            }

            await handler.HandleQueueItemAsync(item);

            // Mark as successfully processed
            await m_queue.MarkAsProcessedAsync(item.LocalId);

            LogInfo($"Successfully processed sync item {item.LocalId}: {item.HandlerType}.{item.Operation}");
            return true;
        }
        catch (ExtendedHttpRequestException ex) when (ex.HttpCode == HttpStatusCode.Conflict)
        {
            LogInfo( $"Sync item {item.LocalId} was superseded by newer server data." );
            await m_queue.MarkAsProcessedAsync( item.LocalId ).ConfigureAwait( false );
            return true;
        }
        catch (Exception ex)
        {
            string errorMessage = $"Error processing sync item {item.LocalId}: {ex.Message}";
            m_loggingService.LogError(ex, errorMessage);
            await m_queue.MarkAsFailedAsync(item.LocalId, errorMessage);
            return false;
        }
    }

    public async Task<SyncStatus> GetSyncStatusAsync()
    {
        List<SyncQueueItem> pendingItems = await m_queue.GetPendingItemsAsync();
        List<SyncQueueItem> failedItems = await m_queue.GetFailedItemsAsync();
        List<SyncQueueItem> stuckItems = await m_queue.GetStuckItemsAsync();

        return new SyncStatus
        {
            PendingCount = pendingItems.Count,
            FailedCount = failedItems.Count,
            StuckCount = stuckItems.Count,
            LastSyncAttempt = DateTime.UtcNow // TODO: Track actual last sync time
        };
    }

    public async Task CleanupOldItemsAsync(TimeSpan failedItemsAge, TimeSpan processedItemsAge)
    {
        await m_queue.CleanupOldFailedItemsAsync(failedItemsAge);
        await m_queue.CleanupOldProcessedItemsAsync(processedItemsAge);
    }
}
