using Principles.Core.Models;

namespace Principles.Core.Services;

public interface ISyncService
{
    Task SyncAsync();
    Task<SyncStatus> GetSyncStatusAsync();
    Task CleanupOldItemsAsync(TimeSpan failedItemsAge, TimeSpan processedItemsAge);
}
