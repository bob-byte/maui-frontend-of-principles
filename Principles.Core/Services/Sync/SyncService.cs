using Principles.Core.Models;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;
using Principles.Core.Services.Managers;

namespace Principles.Core.Services;

public class SyncService
{
    private readonly IEnumerable<ISyncQueueHandler> _handlers;
    private readonly SyncQueueService _queue;

    public SyncService(IEnumerable<ISyncQueueHandler> handlers, SyncQueueService queue)
    {
        _handlers = handlers;
        _queue = queue;
    }

    public async Task ProcessSyncQueueAsync()
    {
        List<SyncQueueItem> items = await _queue.GetPendingItemsAsync();

        foreach (SyncQueueItem item in items)
        {
            await _queue.MarkAsProcessingAsync(item.Id);

            try
            {
                var handler = _handlers.FirstOrDefault(h => h.CanHandle(item.EntityType, item.Operation));
                if (handler is null)
                    continue;

                await handler.HandleAsync(item.PayloadJson);
                await _queue.RemoveFromQueueAsync(item.Id);
            }
            catch (Exception ex)
            {
                // log error
            }
        }
    }
}


