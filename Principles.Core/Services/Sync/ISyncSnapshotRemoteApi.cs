namespace Principles.Core.Services;

public interface ISyncSnapshotRemoteApi
{
    Task<SyncBootstrapResponse> GetBootstrapAsync();
}
