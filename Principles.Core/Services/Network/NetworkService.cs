using Microsoft.Maui.Devices; // Add this for Connectivity
using CommunityToolkit.Maui.Alerts;
using Toast = CommunityToolkit.Maui.Alerts.Toast;

namespace Principles.Core.Services;
public class NetworkService : INetworkService
{
    private readonly ISyncService _syncService;
    private readonly IDatabaseService _localDb;
    private bool wasConnected;


    public NetworkService( ISyncService syncService, IDatabaseService localDb )
    {
        wasConnected = Connectivity.Current.NetworkAccess == NetworkAccess.Internet;
        _syncService = syncService;
        _localDb = localDb;
        // Слухаємо зміни підключення
        Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;
    }

    public async Task CheckInitialConnectionAsync()
    {
        if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
        {
            await ShowGreetingToastAsync();
            await TrySyncAsync();
        }
        else
        {
            await Toast.Make( "Інтернет не доступний" ).Show();
        }
    }

    private async void Connectivity_ConnectivityChanged( object? sender, ConnectivityChangedEventArgs e )
    {
        bool isNowConnected = e.NetworkAccess == NetworkAccess.Internet;

        if (wasConnected != isNowConnected)
        {
            wasConnected = isNowConnected;

            if (isNowConnected)
            {
                await ShowGreetingToastAsync();
                await TrySyncAsync();
            }

            else
                await Toast.Make( "Інтернет втраченo" ).Show();
        }
    }

    private async Task ShowGreetingToastAsync()
    {
        string name = await GetUserNameAsync();
        await Toast.Make( $"Інтернет знову доступний\nВітаю {name}" ).Show();
    }

    private async Task<string> GetUserNameAsync()
    {
        try
        {
            var users = await _localDb.GetAllAsync<UserDto>();
            return users?.FirstOrDefault()?.Name ?? "користувачу";
        }
        catch
        {
            return "користувачу";
        }
    }
    private async Task TrySyncAsync()
    {
        try
        {
            await _syncService.SyncUsersAsync();
        }
        catch (Exception ex)
        {
            await Toast.Make( $"Помилка синхронізації: {ex.Message}" ).Show();
        }
    }
}
