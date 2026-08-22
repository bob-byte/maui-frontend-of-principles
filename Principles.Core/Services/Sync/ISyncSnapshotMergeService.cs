namespace Principles.Core.Services;

public interface ISyncSnapshotMergeService
{
    Task MergeAsync( SyncBootstrapResponse snapshot );
}
