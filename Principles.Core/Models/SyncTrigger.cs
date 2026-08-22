namespace Principles.Core.Models;

public enum SyncTrigger
{
    Startup = 0,
    AuthCompleted = 1,
    Resume = 2,
    ConnectivityRestored = 3
}
