namespace Principles.Core.Models;

public enum SyncRunStatus
{
    Succeeded = 0,
    SkippedNoAuth = 1,
    SkippedNoInternet = 2,
    SkippedBackendUnavailable = 3,
    SkippedThrottled = 4,
    SkippedAlreadyRunning = 5,
    FailedAuthentication = 6,
    Failed = 7
}

public class SyncRunResult
{
    public SyncRunStatus Status { get; init; }
    public DateTime? LastSuccessfulSyncAt { get; init; }
    public DateTime? LastFailedSyncAt { get; init; }
    public Exception? Error { get; init; }
}
