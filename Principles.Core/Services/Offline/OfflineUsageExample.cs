using Principles.Core.Models;

namespace Principles.Core.Services;

/// <summary>
/// Example usage of offline functionality
/// This file demonstrates how to implement offline-first operations in your app
/// </summary>
public static class OfflineUsageExample
{
    /// <summary>
    /// Example: Creating a new habit with offline support
    /// </summary>
    public static async Task CreateHabitWithOfflineSupportAsync(
        IOfflineService offlineService,
        UserHabit habit)
    {
        // This will save locally and queue for sync
        await offlineService.SaveWithOfflineSupportAsync(habit, OperationType.Create);
    }

    /// <summary>
    /// Example: Updating a habit with offline support
    /// </summary>
    public static async Task UpdateHabitWithOfflineSupportAsync(
        IOfflineService offlineService,
        UserHabit habit)
    {
        // This will update locally and queue for sync
        await offlineService.SaveWithOfflineSupportAsync(habit, OperationType.Update);
    }

    /// <summary>
    /// Example: Deleting a habit with offline support
    /// </summary>
    public static async Task DeleteHabitWithOfflineSupportAsync(
        IOfflineService offlineService,
        UserHabit habit)
    {
        // This will delete locally and queue for sync
        await offlineService.SaveWithOfflineSupportAsync(habit, OperationType.Delete);
    }

    /// <summary>
    /// Example: Getting habits with offline fallback
    /// </summary>
    public static async Task<List<UserHabit>> GetHabitsWithOfflineSupportAsync(
        IOfflineService offlineService,
        IDatabaseService databaseService,
        Func<Task<List<UserHabit>>> onlineOperation)
    {
        return await offlineService.ExecuteWithOfflineSupportAsync(
            onlineOperation,
            await databaseService.GetAllAsync<UserHabit>()
        );
    }

    /// <summary>
    /// Example: Checking sync status
    /// </summary>
    public static async Task CheckSyncStatusAsync(IOfflineService offlineService)
    {
        var status = await offlineService.GetSyncStatusAsync();
        
        if (status.PendingCount > 0)
        {
            // Show sync indicator to user
            Console.WriteLine($"Pending sync operations: {status.PendingCount}");
        }
        
        if (status.FailedCount > 0)
        {
            // Show error to user
            Console.WriteLine($"Failed sync operations: {status.FailedCount}");
        }
    }

    /// <summary>
    /// Example: Force sync when user requests it
    /// </summary>
    public static async Task ForceSyncWhenRequestedAsync(IOfflineService offlineService)
    {
        if (offlineService.IsOnline)
        {
            await offlineService.ForceSyncAsync();
        }
        else
        {
            // Show offline message to user
            Console.WriteLine("Cannot sync while offline");
        }
    }
}

/// <summary>
/// Example ViewModel showing how to integrate offline functionality
/// </summary>
public class ExampleOfflineViewModel
{
    private readonly IOfflineService _offlineService;
    private readonly IDatabaseService _databaseService;
    private readonly IServiceOfHabit _habitService;

    public ExampleOfflineViewModel(
        IOfflineService offlineService,
        IDatabaseService databaseService,
        IServiceOfHabit habitService)
    {
        _offlineService = offlineService;
        _databaseService = databaseService;
        _habitService = habitService;
    }

    /// <summary>
    /// Example: Load habits with offline support
    /// </summary>
    public async Task<List<UserHabit>> LoadHabitsAsync()
    {
        return await _offlineService.ExecuteWithOfflineSupportAsync(
            // Online operation
            async () => await _habitService.GetHabitsAsync(),
            // Offline fallback
            await _databaseService.GetAllAsync<UserHabit>()
        );
    }

    /// <summary>
    /// Example: Save habit with offline support
    /// </summary>
    public async Task SaveHabitAsync(UserHabit habit)
    {
        var operation = habit.Id == 0 ? OperationType.Create : OperationType.Update;
        await _offlineService.SaveWithOfflineSupportAsync(habit, operation);
    }

    /// <summary>
    /// Example: Delete habit with offline support
    /// </summary>
    public async Task DeleteHabitAsync(UserHabit habit)
    {
        await _offlineService.SaveWithOfflineSupportAsync(habit, OperationType.Delete);
    }

    /// <summary>
    /// Example: Get sync status for UI
    /// </summary>
    public async Task<SyncStatusInfo> GetSyncStatusAsync()
    {
        return await _offlineService.GetSyncStatusAsync();
    }
} 