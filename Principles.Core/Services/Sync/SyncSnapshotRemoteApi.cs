namespace Principles.Core.Services;

public class SyncSnapshotRemoteApi : BaseRemoteService, ISyncSnapshotRemoteApi
{
    public SyncSnapshotRemoteApi( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
    }

    public Task<SyncBootstrapResponse> GetBootstrapAsync()
    {
        return RequestProvider.GetAsync<SyncBootstrapResponse>( UrlBuilder.SyncBootstrap, SettingsService.AuthAccessToken! );
    }
}
