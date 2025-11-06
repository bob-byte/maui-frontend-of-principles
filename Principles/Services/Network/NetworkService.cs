
namespace Principles.Services;
public class NetworkService : INetworkService
{
    private readonly ITipService m_tipService;

    private bool m_wasConnected;
    
    public NetworkService(IServiceProvider serviceProvider)
    {
        m_wasConnected = IsConnected;

        m_tipService = serviceProvider.GetRequiredService<ITipService>();

        Connectivity.Current.ConnectivityChanged += async ( sender, e ) => await Connectivity_ConnectivityChanged( sender, e ).ConfigureAwait( false );
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
                await ShowGreetingToastAsync().ConfigureAwait( false );
                
                ISyncService syncService = ServiceLocator.Current!.GetRequiredService<ISyncService>();
                syncService.SyncAsync().GetAwaiter();
            }
            else
            {
                await m_tipService.ShowToastAsync( LocStrings.InternetIsLost ).ConfigureAwait( false );
            }
        }
    }

    private Task ShowGreetingToastAsync()
    {
        return m_tipService.ShowToastAsync( LocStrings.InternetIsAvailableAgain );
    }
}
