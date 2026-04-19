namespace Principles.Core.Services;

public interface ISyncReachabilityService
{
    Task<bool> CanReachBackendAsync( CancellationToken cancellationToken = default );
}
