using Principles.Core.Models;
using System.Threading;

namespace Principles.Core.Services;

public class SyncOrchestrator : ISyncOrchestrator
{
    private static readonly TimeSpan ResumeSyncThreshold = TimeSpan.FromSeconds( 30 );

    private readonly INetworkService m_networkService;
    private readonly ISettingsService m_settingsService;
    private readonly ISyncReachabilityService m_reachabilityService;
    private readonly ISyncService m_syncService;
    private readonly ISyncQueueService m_syncQueueService;
    private readonly ILoggingService m_loggingService;
    private int m_isRunning;

    public SyncOrchestrator( IServiceProvider serviceProvider )
    {
        m_networkService = serviceProvider.GetRequiredService<INetworkService>();
        m_settingsService = serviceProvider.GetRequiredService<ISettingsService>();
        m_reachabilityService = serviceProvider.GetRequiredService<ISyncReachabilityService>();
        m_syncService = serviceProvider.GetRequiredService<ISyncService>();
        m_syncQueueService = serviceProvider.GetRequiredService<ISyncQueueService>();
        m_loggingService = serviceProvider.GetRequiredService<ILoggingService>();
    }

    public async Task<SyncRunResult> RunAsync( SyncTrigger trigger, CancellationToken cancellationToken = default )
    {
        if (Interlocked.Exchange( ref m_isRunning, 1 ) == 1)
        {
            return CreateResult( SyncRunStatus.SkippedAlreadyRunning );
        }

        try
        {
            string authToken = m_settingsService.AuthAccessToken ?? await m_settingsService.GetAuthAccessTokenAsync().ConfigureAwait( false );
            if (string.IsNullOrWhiteSpace( authToken ))
            {
                return CreateResult( SyncRunStatus.SkippedNoAuth );
            }

            if (trigger == SyncTrigger.Resume && !await ShouldRunOnResumeAsync().ConfigureAwait( false ))
            {
                return CreateResult( SyncRunStatus.SkippedThrottled );
            }

            if (!m_networkService.IsConnected)
            {
                return CreateResult( SyncRunStatus.SkippedNoInternet );
            }

            bool canReachBackend = await m_reachabilityService.CanReachBackendAsync( cancellationToken ).ConfigureAwait( false );
            if (!canReachBackend)
            {
                DateTime failureTime = DateTime.UtcNow;
                m_settingsService.LastFailedSyncAt = failureTime;
                return CreateResult( SyncRunStatus.SkippedBackendUnavailable );
            }

            await m_syncService.SyncAsync().ConfigureAwait( false );
            m_settingsService.LastSuccessfulSyncAt = DateTime.UtcNow;
            m_settingsService.LastFailedSyncAt = null;

            return CreateResult( SyncRunStatus.Succeeded );
        }
        catch (ServiceAuthenticationException ex)
        {
            m_settingsService.LastFailedSyncAt = DateTime.UtcNow;
            m_loggingService.LogError( ex, "Authentication failed during sync orchestration." );
            return CreateResult( SyncRunStatus.FailedAuthentication, ex );
        }
        catch (Exception ex)
        {
            m_settingsService.LastFailedSyncAt = DateTime.UtcNow;
            m_loggingService.LogError( ex, "Sync orchestration failed." );
            return CreateResult( SyncRunStatus.Failed, ex );
        }
        finally
        {
            Interlocked.Exchange( ref m_isRunning, 0 );
        }
    }

    private async Task<bool> ShouldRunOnResumeAsync()
    {
        List<SyncQueueItem> blockingItems = await m_syncQueueService.GetBlockingItemsAsync().ConfigureAwait( false );
        if (blockingItems.Count > 0)
        {
            return true;
        }

        DateTime? lastSuccessfulSyncAt = m_settingsService.LastSuccessfulSyncAt;
        return lastSuccessfulSyncAt is null || DateTime.UtcNow - lastSuccessfulSyncAt.Value >= ResumeSyncThreshold;
    }

    private SyncRunResult CreateResult( SyncRunStatus status, Exception? error = null )
    {
        return new SyncRunResult
        {
            Status = status,
            LastSuccessfulSyncAt = m_settingsService.LastSuccessfulSyncAt,
            LastFailedSyncAt = m_settingsService.LastFailedSyncAt,
            Error = error
        };
    }
}
