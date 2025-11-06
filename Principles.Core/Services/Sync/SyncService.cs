using Principles.Core.Models;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;

namespace Principles.Core.Services;

public class SyncService : ISyncService
{
    private readonly List<ISyncQueueHandler> m_handlers;
    private readonly ISyncQueueService m_queue;
    private readonly ILoggingService m_loggingService;
    private readonly ISettingsService m_settingsService;
    private readonly INetworkService m_networkService;

    public SyncService(IServiceProvider serviceProvider)
    {
        m_queue = serviceProvider.GetRequiredService<ISyncQueueService>();
        m_loggingService = serviceProvider.GetRequiredService<ILoggingService>();
        m_settingsService = serviceProvider.GetRequiredService<ISettingsService>();
        m_networkService = serviceProvider.GetRequiredService<INetworkService>();
        
        Type handlerInterfaceType = typeof(ISyncQueueHandler);
        
        // Find all types that implement ISyncQueueHandler
        Type[] handlerTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => handlerInterfaceType.IsAssignableFrom(t) && t is { IsInterface: false, IsAbstract: false })
            .ToArray();
        
        m_handlers = [];

        foreach (Type? type in handlerTypes.Select( t => t.BaseType))
        {
            if (type is null)
            {
                throw new InvalidOperationException( "Handler type base is null. It should be RemoteApiService<Type>" );
            }

            ISyncQueueHandler handler = serviceProvider.GetRequiredService( type ) as ISyncQueueHandler ?? throw new InvalidOperationException( message: $"Handler \"{type.FullName}\" doens't implement interface  {nameof(ISyncQueueHandler)}" );
            m_handlers.Add(handler);
        }
    }

    public async Task SyncAsync()
    {
        if (!m_networkService.IsConnected)
        {
            return;
        }
        
        try
        {
            // Reset any stuck items before processing
            await m_queue.ResetStuckItemsAsync();
            
            List<SyncQueueItem> items = await m_queue.GetPendingItemsAsync();
            
            if (items.Count == 0)
            {
                LogInfo("No pending sync items to process");
                
                return;
            }

            LogInfo($"Processing {items.Count} sync items");

            foreach (SyncQueueItem item in items)
            {
                await ProcessQueueItemAsync(item);
            }
        }
        catch (Exception ex)
        {
            m_loggingService.LogError(ex, "Error during sync process");
            throw;
        }
    }

    private void LogInfo( string message )
    {
        if (m_settingsService.IsDebug)
        {
            m_loggingService.LogInfo(message);
        }
    }

    private async Task ProcessQueueItemAsync(SyncQueueItem item)
    {
        try
        {
            await m_queue.MarkAsProcessingAsync(item.LocalId);
            
            LogInfo($"Processing sync item {item.LocalId}: {item.HandlerType}.{item.Operation}");

            ISyncQueueHandler? handler = m_handlers.FirstOrDefault(h => h.CanHandle(item.HandlerType));
            if (handler is null)
            {
                string errorMessage = $"Handler for entity \"{item.HandlerType}\" not found";
                m_loggingService.LogFatal(errorMessage);
                await m_queue.MarkAsFailedAsync(item.LocalId, errorMessage);
                return;
            }

            // Process the operation based on type
            if (item.Operation == OperationType.Save.ToString())
            {
                if (item.EntityId.HasValue && item.EntityLocalId.HasValue)
                {
                    await handler.SaveAsync(item.EntityId.Value, item.EntityLocalId.Value, item.PayloadJson);
                }
                else
                {
                    string errorMessage = "Save operation missing required EntityId or EntityLocalId";
                    m_loggingService.LogFatal(errorMessage);
                    await m_queue.MarkAsFailedAsync(item.LocalId, errorMessage);
                    return;
                }
            }
            else if (item.Operation == OperationType.Delete.ToString())
            {
                if (item.EntityId.HasValue)
                {
                    await handler.DeleteAsync(item.EntityId.Value, item.PayloadJson);
                }
                else
                {
                    string errorMessage = "Delete operation missing required EntityId";
                    m_loggingService.LogError(errorMessage);
                    await m_queue.MarkAsFailedAsync(item.LocalId, errorMessage);
                    return;
                }
            }
            else
            {
                // Custom operation
                await handler.ExecuteAsync(item.Operation, item.PayloadJson);
            }

            // Mark as successfully processed
            await m_queue.MarkAsProcessedAsync(item.LocalId);
            
            LogInfo($"Successfully processed sync item {item.LocalId}: {item.HandlerType}.{item.Operation}");
        }
        catch (Exception ex)
        {
            string errorMessage = $"Error processing sync item {item.LocalId}: {ex.Message}";
            m_loggingService.LogError(ex, errorMessage);
            await m_queue.MarkAsFailedAsync(item.LocalId, errorMessage);
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


