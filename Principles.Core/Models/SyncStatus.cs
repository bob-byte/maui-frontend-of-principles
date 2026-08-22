namespace Principles.Core.Models;

public class SyncStatus
{
    public int PendingCount { get; set; }
    public int FailedCount { get; set; }
    public int StuckCount { get; set; }
    public DateTime LastSyncAttempt { get; set; }
}
