using Microsoft.Maui.Devices; // Add this for Connectivity
using CommunityToolkit.Maui.Alerts;
using Toast = CommunityToolkit.Maui.Alerts.Toast;

namespace Principles.Core.Services;
public class NetworkService : INetworkService
{
    private bool m_wasConnected;
    private readonly ISyncService m_syncService;
    
    public NetworkService(IServiceProvider serviceProvider)
    {
        m_wasConnected = IsConnected;
        
        m_syncService = serviceProvider.GetRequiredService<ISyncService>();
        
        Connectivity.Current.ConnectivityChanged += async (sender, e) => await Connectivity_ConnectivityChanged(sender, e);
    }
    
    public bool IsConnected => Connectivity.Current.NetworkAccess == NetworkAccess.Internet;

    private async Task Connectivity_ConnectivityChanged( object? sender, ConnectivityChangedEventArgs e )
    {
        bool isNowConnected = e.NetworkAccess == NetworkAccess.Internet;

        if (m_wasConnected != isNowConnected)
        {
            m_wasConnected = isNowConnected;

            if (isNowConnected)
            {
                m_syncService.SyncAsync().GetAwaiter();
                await ShowGreetingToastAsync();
            }
            else
            {
                await Toast.Make( "Інтернет втраченo" ).Show();
            }
        }
    }

    private async Task ShowGreetingToastAsync()
    {
        await Toast.Make( $"Інтернет знову доступний" ).Show();
    }
}
