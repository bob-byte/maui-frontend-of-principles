namespace Principles.Core.Models;

public class SyncRetryConfig
{
    public int MaxRetryAttempts { get; set; } = 3;
    public TimeSpan BaseDelay { get; set; } = TimeSpan.FromMinutes(1);
    public TimeSpan MaxDelay { get; set; } = TimeSpan.FromHours(1);
    public double BackoffMultiplier { get; set; } = 2.0;
    public TimeSpan JitterRange { get; set; } = TimeSpan.FromSeconds(30);
    
    public static SyncRetryConfig Default => new();
} 