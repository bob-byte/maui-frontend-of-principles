## Offline Mode Implementation Guide

## Overview

This document describes the current offline mode implementation for the Principles app. It uses a lightweight local repository backed by SQLite and a sync queue processed by handlers when connectivity is available.

## Architecture

### Core Components (actual classes)

1. **Local Repository** (`IOfflineRepository` / `OfflineRepository`)
   - Generic CRUD over SQLite via `sqlite-net-pcl`
   - Auto-creates tables for all types implementing `IOfflineEntity`
   - Supports sync and async operations, simple field queries, and raw SQL

2. **Remote API abstraction** (`IRemoteApiService<T>` / `RemoteApiService<T>`)
   - Typed remote operations for entities (`T : IEntity`)

3. **Offline-aware API** (`IOfflineApiService<T>` / `OfflineApiService<T>`)
   - Wraps local repository + remote API + sync queue
   - Decides when to read remote vs local; queues writes while offline

4. **Sync Queue** (`ISyncQueueService` / `SyncQueueService`)
   - Persists pending operations as `SyncQueueItem` in local DB
   - **Enhanced retry logic** with exponential backoff and jitter
   - **Failure lifecycle management** with proper error tracking
   - **Stuck item recovery** and automatic cleanup strategies

5. **Sync Orchestrator** (`ISyncService` / `SyncService`)
   - Discovers and invokes `ISyncQueueHandler` implementations
   - **Comprehensive error handling** with proper logging
   - **Stuck item detection and recovery**
   - **Sync status monitoring** and cleanup operations

6. **Connectivity** (`INetworkService` / `NetworkService`)
   - Indicates whether device is online; used by `OfflineApiService<T>`

### Key Models

- `IOfflineEntity`
  - Marker with `[PrimaryKey, AutoIncrement] long LocalId { get; set; }`

- `OperationType`
  - Values: `Save`, `Delete`

- `SyncQueueItem`
  - Fields:
    - `long LocalId`
    - `long? EntityId`
    - `long? EntityLocalId`
    - `string HandlerType` (required)
    - `string Operation` (required, typically `Save` or `Delete`)
    - `string? PayloadJson`
    - `DateTime LastModified` (UTC, set on create)
    - `bool IsProcessing`
    - `bool IsProcessed`
    - **`int RetryCount`** - tracks retry attempts
    - **`DateTime? NextRetryAt`** - when to retry next
    - **`DateTime? LastRetryAt`** - when last retry occurred
    - **`string? ErrorMessage`** - last error details
    - **`DateTime? ProcessedAt`** - when successfully processed
    - **`bool IsFailed`** - permanent failure flag

- **`SyncRetryConfig`** - configurable retry behavior
  - `MaxRetryAttempts` (default: 3)
  - `BaseDelay` (default: 1 minute)
  - `MaxDelay` (default: 1 hour)
  - `BackoffMultiplier` (default: 2.0)
  - `JitterRange` (default: 30 seconds)

- **`SyncStatus`** - sync operation status
  - `PendingCount` - items waiting to be processed
  - `FailedCount` - permanently failed items
  - `StuckCount` - items stuck in processing
  - `LastSyncAttempt` - when last sync occurred

## Data Flow

### Read (GetAll)
1. If online and `forceRefresh == true`, fetch remote list, persist locally, return remote
2. Otherwise, read from local repository

### Write (Save/Delete/Execute)
1. Apply local change first (insert/update/delete via `IOfflineRepository`)
2. If online, attempt remote call; on failure, enqueue
3. If offline, enqueue directly
4. `ISyncService.SyncAsync()` later processes the queue using registered handlers

### Enhanced Sync Process
1. **Pre-sync cleanup**: Reset any stuck items (>30 minutes in processing)
2. **Smart filtering**: Only process items due for retry (`NextRetryAt <= now`)
3. **Proper lifecycle**: Mark as processing → execute → mark as processed/failed
4. **Retry management**: Exponential backoff with jitter prevents thundering herd
5. **Error handling**: Comprehensive logging and error tracking
6. **Cleanup strategies**: Configurable retention policies for failed/processed items

## Usage Examples (reflecting current APIs)

### Inject and use `OfflineApiService<T>`
```csharp
private readonly IOfflineApiService<UserHabit> _offlineApi; // resolved from DI

public async Task<List<UserHabit>> LoadHabitsAsync(bool forceRefresh)
{
    return await _offlineApi.GetAllAsync(forceRefresh);
}

public async Task SaveHabitAsync(UserHabit habit)
{
    await _offlineApi.SaveAsync(habit);
}

public async Task DeleteHabitAsync(UserHabit habit)
{
    await _offlineApi.DeleteAsync(habit);
}
```

### Execute custom operation with local-first change
```csharp
// handlerType must match the handler's CanHandle() type key
await _offlineApi.ExecuteAsync(
    localChange: () => _localRepository.UpdateAsync(habit),
    handlerType: nameof(UserHabit),
    operation: "CustomOperation",
    data: new { habitId = habit.Id, value = 123 }
);
```

### Processing the queue with enhanced features
```csharp
// Typically called when app gains connectivity, or on a timer
await _syncService.SyncAsync();

// Monitor sync status
var status = await _syncService.GetSyncStatusAsync();
if (status.FailedCount > 0)
{
    // Handle failed operations
    Console.WriteLine($"Failed operations: {status.FailedCount}");
}

// Cleanup old items (failed items older than 7 days, processed items older than 1 day)
await _syncService.CleanupOldItemsAsync(
    TimeSpan.FromDays(7), 
    TimeSpan.FromDays(1)
);
```

### Advanced queue management
```csharp
// Get stuck items and reset them
var stuckItems = await _syncQueueService.GetStuckItemsAsync();
if (stuckItems.Count > 0)
{
    await _syncQueueService.ResetStuckItemsAsync();
}

// Manual cleanup of old failed items
await _syncQueueService.CleanupOldFailedItemsAsync(TimeSpan.FromDays(30));
```

## Implementation Details

### `OfflineRepository`
- Initializes SQLite connections (sync/async) with flags: ReadWrite, Create, SharedCache, FullMutex
- Creates tables for all `IOfflineEntity` implementations at startup
- Provides `SaveAsync`, `Insert*`, `Update*`, `Delete*`, `Get*`, `Where*`, `Upsert*`, transaction helpers, and raw SQL

### `OfflineApiService<T>`
- `GetAllAsync(forceRefresh)`; refreshes local cache from remote when online and requested
- `SaveAsync(item)`; saves locally, then remote or queue
- `DeleteAsync(item)`; deletes locally, then remote or queue
- `ExecuteAsync(localChange, handlerType, operation, data)`; runs local change, then remote call or queues executable task

### `SyncQueueService` (Enhanced)
- **Smart queue filtering**: Only returns items ready for processing
- **Retry lifecycle**: Tracks retry counts, schedules next attempts with exponential backoff
- **Failure management**: Marks items as failed after max retries, provides error details
- **Stuck item recovery**: Detects and resets items stuck in processing
- **Cleanup strategies**: Configurable retention policies for different item states

### `SyncService` (Enhanced)
- **Pre-sync cleanup**: Automatically resets stuck items before processing
- **Comprehensive error handling**: Logs all operations and errors via `ILoggingService`
- **Proper lifecycle management**: Uses `MarkAsProcessedAsync` and `MarkAsFailedAsync`
- **Status monitoring**: Provides detailed sync status information
- **Cleanup operations**: Orchestrates cleanup of old items

### `ISyncQueueHandler`
- Contract:
```csharp
bool CanHandle(string handlerType);
Task SaveAsync(long entityId, long localId, string? payloadJson);
Task DeleteAsync(long entityId, string? payloadJson);
Task ExecuteAsync(string operation, string? payloadJson);
```

## Dependency Injection Registration

Registered in `Principles.Core/Extensions/ServiceCollectionExtensions.cs`:
```csharp
services.AddSingleton<INetworkService, NetworkService>();
services.AddSingleton<ISyncService, SyncService>();
services.AddSingleton<ISyncQueueService, SyncQueueService>();
services.AddSingleton<IOfflineRepository, OfflineRepository>();
services.AddSingleton<RemoteApiService<UserHabit>, HabitRemoteApi>();
```

Note: `OfflineApiService<T>` is constructed via `IServiceProvider` in its constructor and expects `IOfflineRepository`, `IRemoteApiService<T>`, `ISyncQueueService`, and `INetworkService` to be registered.

## Testing

### Offline
1. Disable connectivity
2. Perform save/delete/execute operations — verify local DB is updated and queue items are created
3. Re-enable connectivity
4. Trigger `SyncAsync()` and verify items are processed and removed

### Online with transient failures
1. Stay online but simulate server failure
2. Verify the operation is queued and processed later
3. Check retry behavior with exponential backoff

### Retry and failure scenarios
1. Simulate handler failures to test retry logic
2. Verify items are marked as failed after max retries
3. Test stuck item recovery and cleanup operations

## Troubleshooting

- Verify `HandlerType` in queue items matches a handler's `CanHandle`
- Ensure entity types used in local storage implement `IOfflineEntity` so tables are created
- Check `IsProcessing` stuck items and logs in `SyncService` catch blocks
- Monitor retry counts and exponential backoff timing
- Use `GetSyncStatusAsync()` to monitor queue health
- Reset stuck items with `ResetStuckItemsAsync()` if needed

## Notes / Future Work

- **Retry/backoff system** is now fully implemented with configurable parameters
- **Comprehensive logging** via `ILoggingService` for debugging and monitoring
- **Stuck item detection and recovery** prevents sync from getting blocked
- **Cleanup strategies** prevent database bloat while maintaining audit trails
- **Sync status monitoring** provides visibility into queue health
- **Conflict resolution strategy** is handler-defined; consider explicit strategies if needed
- **Background sync** could be implemented using the new status monitoring capabilities