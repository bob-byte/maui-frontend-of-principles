using Microsoft.Maui.ApplicationModel;

using Principles.Core.Models;

namespace Principles.Services;

public class NetworkService : INetworkService
{
    private readonly ITipService m_tipService;
    private readonly IServiceProvider m_serviceProvider;
    private readonly ISettingsService m_settingsService;

    private bool m_wasConnected;

    public NetworkService( IServiceProvider serviceProvider )
    {
        m_serviceProvider = serviceProvider;
        m_settingsService = serviceProvider.GetRequiredService<ISettingsService>();
        m_wasConnected = IsConnected;

        m_tipService = serviceProvider.GetRequiredService<ITipService>();

        Connectivity.Current.ConnectivityChanged += async ( sender, e ) => await Connectivity_ConnectivityChanged( sender, e ).ConfigureAwait( false );
    }

    public bool IsConnected => Connectivity.Current.NetworkAccess == NetworkAccess.Internet;

    private async Task Connectivity_ConnectivityChanged( object? sender, ConnectivityChangedEventArgs e )
    {
        bool isNowConnected = e.NetworkAccess == NetworkAccess.Internet;

        if (!m_wasConnected && isNowConnected)
        {
            try
            {
                ISyncOrchestrator syncOrchestrator = m_serviceProvider.GetRequiredService<ISyncOrchestrator>();
                SyncRunResult syncResult = await syncOrchestrator.RunAsync( SyncTrigger.ConnectivityRestored ).ConfigureAwait( false );
                if (syncResult.Status == SyncRunStatus.FailedAuthentication)
                {
                    await HandleAuthenticationFailureAsync().ConfigureAwait( false );
                }
            }
            catch
            {
                // Sync is best-effort here; the next trigger will retry.
            }
        }

        if (m_wasConnected && !isNowConnected)
        {
            await m_tipService.ShowToastAsync( LocStrings.NoInternetConnection ).ConfigureAwait( false );
        }

        m_wasConnected = isNowConnected;
    }

    private async Task HandleAuthenticationFailureAsync()
    {
        IUserService userService = m_serviceProvider.GetRequiredService<IUserService>();
        INavigationService navigationService = m_serviceProvider.GetRequiredService<INavigationService>();

        await m_settingsService.SetAuthAccessTokenAsync( string.Empty ).ConfigureAwait( false );
        await userService.ClearLocalDataAsync().ConfigureAwait( false );
        await MainThread.InvokeOnMainThreadAsync( () => navigationService.GoToInitialViewAsync() );
    }
}