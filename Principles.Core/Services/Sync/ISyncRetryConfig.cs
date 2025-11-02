namespace Principles.Core.Services;

public interface ISyncRetryConfig
{
    int MaxRetryAttempts { get; set; }
    TimeSpan BaseDelay { get; set; }
    TimeSpan MaxDelay { get; set; }
    double BackoffMultiplier { get; set; }
    TimeSpan JitterRange { get; set; }
}