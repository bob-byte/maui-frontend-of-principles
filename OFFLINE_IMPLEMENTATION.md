# Offline Mode Implementation Guide

## Overview

This document describes the offline mode implementation for the Principles app. The implementation provides a robust offline-first architecture using SQLite for local storage and a sync queue system for data synchronization.

## Architecture

### Core Components

1. **SQLite Database** (`DatabaseService`)
   - Local storage using `sqlite-net-pcl`
   - ACID compliant transactions
   - Cross-platform support (iOS/Android)

2. **Sync Queue System** (`SyncQueueService`)
   - Queues offline operations for later sync
   - Handles Create, Update, Delete operations
   - Retry mechanism with exponential backoff

3. **Enhanced Sync Service** (`EnhancedSyncService`)
   - Processes queued operations when online
   - Handles network connectivity
   - Provides sync status information

4. **Offline Service** (`OfflineService`)
   - High-level interface for offline operations
   - Automatic fallback to local data
   - Network connectivity monitoring

## Database Choice: SQLite

**Why SQLite is the best choice for this app:**

### ✅ Advantages
- **Already integrated** - Using `sqlite-net-pcl` which is perfect for .NET MAUI
- **Cross-platform** - Works seamlessly on iOS and Android
- **Lightweight** - Minimal memory footprint (~500KB)
- **ACID compliance** - Ensures data integrity
- **No server required** - Perfect for offline-first architecture
- **Mature ecosystem** - Well-tested with existing packages
- **Performance** - Fast read/write operations
- **File-based** - Easy backup and restore

### 🔄 Alternative Considerations
- **Realm**: More complex, better for complex relationships, but overkill
- **Entity Framework Core**: Heavier, better for complex queries, but SQLite-net-pcl is more suitable for mobile
- **Custom JSON storage**: Less robust, no ACID compliance

## Usage Examples

### Basic Offline Operations

```csharp
// Inject the service
private readonly IOfflineService _offlineService;

// Create a new habit with offline support
public async Task CreateHabitAsync(UserHabit habit)
{
    await _offlineService.SaveWithOfflineSupportAsync(habit, OperationType.Create);
}

// Update a habit with offline support
public async Task UpdateHabitAsync(UserHabit habit)
{
    await _offlineService.SaveWithOfflineSupportAsync(habit, OperationType.Update);
}

// Delete a habit with offline support
public async Task DeleteHabitAsync(UserHabit habit)
{
    await _offlineService.SaveWithOfflineSupportAsync(habit, OperationType.Delete);
}
```

### Data Retrieval with Offline Fallback

```csharp
public async Task<List<UserHabit>> GetHabitsAsync()
{
    return await _offlineService.ExecuteWithOfflineSupportAsync(
        // Online operation
        async () => await _habitService.GetHabitsAsync(),
        // Offline fallback
        await _databaseService.GetAllAsync<UserHabit>()
    );
}
```

### Sync Status Monitoring

```csharp
public async Task CheckSyncStatusAsync()
{
    var status = await _offlineService.GetSyncStatusAsync();
    
    if (status.PendingCount > 0)
    {
        // Show sync indicator
        ShowSyncIndicator(status.PendingCount);
    }
    
    if (status.FailedCount > 0)
    {
        // Show error message
        ShowSyncError(status.LastError);
    }
}
```

## Implementation Details

### Sync Queue Item Structure

```csharp
public class SyncQueueItem
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    
    public string EntityType { get; set; }           // e.g., "UserHabit"
    public OperationType Operation { get; set; }     // Create, Update, Delete
    public string PayloadJson { get; set; }          // Serialized entity
    public DateTime CreatedAt { get; set; }
    
    public bool IsProcessing { get; set; } = false;
    public bool IsProcessed { get; set; } = false;
    
    public string? EntityId { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public int RetryCount { get; set; } = 0;
    public DateTime? LastRetryAt { get; set; }
}
```

### Sync Process Flow

1. **User performs action** (create/update/delete)
2. **Save locally** - Data is immediately saved to SQLite
3. **Queue for sync** - Operation is added to sync queue
4. **Network check** - If online, attempt immediate sync
5. **Background sync** - When network becomes available, process queue
6. **Retry logic** - Failed operations are retried with backoff
7. **Cleanup** - Old processed operations are cleaned up

### Error Handling

- **Network failures**: Operations are queued for later sync
- **API failures**: Retry with exponential backoff (max 3 attempts)
- **Data conflicts**: Currently uses "last write wins" strategy
- **Corruption**: SQLite provides ACID compliance for data integrity

## Configuration

### Service Registration

Services are already registered in `ServiceCollectionExtensions.cs`:

```csharp
services.AddSingleton<IDatabaseService, DatabaseService>();
services.AddSingleton<ISyncQueueService, SyncQueueService>();
services.AddSingleton<IEnhancedSyncService, EnhancedSyncService>();
services.AddSingleton<IOfflineService, OfflineService>();
```

### Database Initialization

The database is automatically initialized in `DatabaseService`:

```csharp
public async Task InitializeAsync()
{
    await _db.CreateTableAsync<UserInfo>();
    await _db.CreateTableAsync<SyncQueueItem>();
    // Add other tables as needed
}
```

## Best Practices

### 1. Always Use Offline Service
```csharp
// ✅ Good
await _offlineService.SaveWithOfflineSupportAsync(habit, OperationType.Create);

// ❌ Bad - Direct API calls without offline support
await _habitService.CreateHabitAsync(habit);
```

### 2. Handle Sync Status in UI
```csharp
// Show sync indicator when there are pending operations
var status = await _offlineService.GetSyncStatusAsync();
if (status.PendingCount > 0)
{
    ShowSyncIndicator();
}
```

### 3. Provide User Feedback
```csharp
if (!_offlineService.IsOnline)
{
    await ShowMessageAsync("You're offline. Changes will sync when you're back online.");
}
```

### 4. Regular Cleanup
```csharp
// Clean up old sync data periodically
await _offlineService.CleanupOldDataAsync();
```

## Testing

### Offline Testing
1. Enable airplane mode
2. Perform operations (create/update/delete)
3. Verify data is saved locally
4. Disable airplane mode
5. Verify sync occurs automatically

### Sync Testing
1. Perform operations while offline
2. Check sync queue in database
3. Go online and verify sync
4. Check for any failed operations

## Troubleshooting

### Common Issues

1. **Sync not working**
   - Check network connectivity
   - Verify sync queue has items
   - Check logs for errors

2. **Data not appearing offline**
   - Ensure data is saved locally first
   - Check database initialization
   - Verify table creation

3. **Performance issues**
   - Clean up old sync data
   - Limit sync queue size
   - Optimize database queries

### Debug Commands

```csharp
// Check sync status
var status = await _offlineService.GetSyncStatusAsync();

// Force sync
await _offlineService.ForceSyncAsync();

// Check pending operations
var pending = await _syncQueueService.GetPendingOperationsAsync();

// Clean up old data
await _offlineService.CleanupOldDataAsync();
```

## Future Enhancements

1. **Conflict Resolution**: Implement user-choice conflict resolution
2. **Selective Sync**: Sync only specific data types
3. **Compression**: Compress sync payloads
4. **Incremental Sync**: Only sync changed data
5. **Background Sync**: Periodic background sync
6. **Sync Analytics**: Track sync performance and errors

## Conclusion

The offline implementation provides a robust, user-friendly experience that works seamlessly whether the user is online or offline. SQLite is the perfect choice for this implementation, offering the right balance of performance, reliability, and simplicity.

The architecture is designed to be:
- **Reliable**: ACID compliance ensures data integrity
- **Fast**: Local operations are immediate
- **User-friendly**: Seamless online/offline transitions
- **Maintainable**: Clean separation of concerns
- **Scalable**: Easy to add new entity types 