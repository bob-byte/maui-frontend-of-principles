using System.Net;
using System.Net.Http.Headers;

namespace Principles.Core.Services;

public class SyncReachabilityService : ISyncReachabilityService
{
    private static readonly TimeSpan PingTimeout = TimeSpan.FromSeconds( 5 );

    private readonly IHttpClientFactory m_httpClientFactory;
    private readonly ISettingsService m_settingsService;
    private readonly IUrlBuilder m_urlBuilder;
    private readonly ILoggingService m_loggingService;

    public SyncReachabilityService( IServiceProvider serviceProvider )
    {
        m_httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        m_settingsService = serviceProvider.GetRequiredService<ISettingsService>();
        m_urlBuilder = serviceProvider.GetRequiredService<IUrlBuilder>();
        m_loggingService = serviceProvider.GetRequiredService<ILoggingService>();
    }

    public async Task<bool> CanReachBackendAsync( CancellationToken cancellationToken = default )
    {
        string authToken = m_settingsService.AuthAccessToken ?? await m_settingsService.GetAuthAccessTokenAsync().ConfigureAwait( false );
        if (string.IsNullOrWhiteSpace( authToken ))
        {
            return false;
        }

        using CancellationTokenSource timeoutCts = CancellationTokenSource.CreateLinkedTokenSource( cancellationToken );
        timeoutCts.CancelAfter( PingTimeout );

        try
        {
            HttpClient httpClient = m_httpClientFactory.CreateClient( nameof( RequestProvider ) );
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue( "Bearer", authToken );

            using HttpResponseMessage response = await httpClient.GetAsync( m_urlBuilder.SyncPing, timeoutCts.Token ).ConfigureAwait( false );
            if (response.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.Unauthorized)
            {
                throw new ServiceAuthenticationException( "SyncPingUnauthorized" );
            }

            return response.IsSuccessStatusCode;
        }
        catch (ServiceAuthenticationException)
        {
            throw;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            m_loggingService.LogInfo( $"Sync ping failed: {ex.Message}" );
            return false;
        }
    }
}
