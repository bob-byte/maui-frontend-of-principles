using Microsoft.Extensions.DependencyInjection;

using Principles.Core.Extensions;
using Principles.Core.Models;
using Principles.Core.Services;

using Serilog;

namespace Principles.UnitTests;

public class Config
{
    static Config()
    {
        SetupSerilog();

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<ISettingsService, SimpleSettingsService>();
        serviceCollection.AddSingleton<IPreferencesService, InMemoryPreferencesService>();
        serviceCollection.AddSingleton<ISecureStorageService, InMemorySecureStorageService>();
        serviceCollection.AddSingleton<IDatabasePathProvider, TempDatabasePathProvider>();
        serviceCollection.AddSingleton<IDatabaseKeyProvider, TestDatabaseKeyProvider>();
        serviceCollection.AddSingleton<INetworkService, AlwaysOnlineNetworkService>();
        serviceCollection.AddSingleton<IReminderService, NullReminderService>();
        serviceCollection.AddSingleton<IAppOpenTrackerService, NullAppOpenTrackerService>();
        serviceCollection.RegisterAppCore();

        ServiceProvider = serviceCollection.BuildServiceProvider();
    }

    public static IServiceProvider ServiceProvider { get; }

    private static void SetupSerilog()
    {
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .CreateLogger();
    }
}

internal sealed class InMemoryPreferencesService : IPreferencesService
{
    private readonly Dictionary<string, object> m_values = new();

    public string GetStoredValue( string key ) =>
        m_values.TryGetValue( key, out object? value ) ? value?.ToString() ?? string.Empty : string.Empty;

    public bool GetStoredValueOrDefault( string key, bool defaultValue = false ) =>
        m_values.TryGetValue( key, out object? value ) && value is bool b ? b : defaultValue;

    public void SetForever( string key, string value ) => m_values[key] = value;

    public void SetForever( string key, bool value ) => m_values[key] = value;

    public void Remove( string key ) => m_values.Remove( key );
}

internal sealed class InMemorySecureStorageService : ISecureStorageService
{
    private readonly Dictionary<string, string> m_values = new();

    public Task<string?> GetAsync( string key ) =>
        Task.FromResult( m_values.TryGetValue( key, out string? value ) ? value : null );

    public Task SetAsync( string key, string value )
    {
        m_values[key] = value;
        return Task.CompletedTask;
    }
}

internal sealed class TempDatabasePathProvider : IDatabasePathProvider
{
    public string GetDatabasePath() =>
        Path.Combine( Path.GetTempPath(), $"principles-unit-tests-{Guid.NewGuid():N}.db3" );
}

internal sealed class TestDatabaseKeyProvider : IDatabaseKeyProvider
{
    public string GetDatabaseKey() => "unit-test-db-key";
}

internal sealed class AlwaysOnlineNetworkService : INetworkService
{
    public bool IsConnected => true;
}

internal sealed class NullAppOpenTrackerService : IAppOpenTrackerService
{
    public void TrackAppOpen()
    {
    }

    public DateTime? GetLastOpenDate() => null;

    public DateTime? GetLastMissedDate() => null;

    public int DaysSinceLastOpen() => 0;
}

internal sealed class NullReminderService : IReminderService
{
    public bool IsLocalNotificationSupported() => false;

    public Task CancelLocallyAsync( int id ) => Task.CompletedTask;

    public Task CancelAllLocallyAsync() => Task.CompletedTask;

    public Task ClearDeliveredLocallyAsync() => Task.CompletedTask;

    public Task SaveLocallyAsync( int id, string title, string description, DateTime notifyTime, ReminderRepeat repeatType ) =>
        Task.CompletedTask;

    public Task RenamePendingNotificationTitlesAsync( string oldTitle, string newTitle ) => Task.CompletedTask;

    public Task TryToRecoverAllUserRemindersAsync() => Task.CompletedTask;

    public Task<Reminder> HabitsReportReminderAsync() => Task.FromResult( new Reminder() );

    public Task<SaveHabitsReportReminderResponse> SaveHabitsReportReminderAsync( Reminder reminder ) =>
        Task.FromResult( new SaveHabitsReportReminderResponse() );

    public Task<bool> RequestAccessToSendNotificationsAsync() => Task.FromResult( false );

    public void Cancel( int id )
    {
    }

    public Task AddNotificationToDeviceAsync( bool isNewHabit, UserHabitReminder reminder, WeekDay weekDay ) =>
        Task.CompletedTask;
}
