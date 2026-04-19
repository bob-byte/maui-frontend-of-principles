using Principles.Core.Models;

namespace Principles.Core.Services;

public interface ISyncOrchestrator
{
    Task<SyncRunResult> RunAsync( SyncTrigger trigger, CancellationToken cancellationToken = default );
}
