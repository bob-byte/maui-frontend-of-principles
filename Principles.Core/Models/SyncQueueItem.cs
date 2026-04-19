namespace Principles.Core.Models;

public class SyncQueueItem : IOfflineEntity
{
    [PrimaryKey, AutoIncrement]
    public long LocalId { get; set; }

    public long? EntityId { get; set; }
    public long? EntityLocalId { get; set; }

    [NotNull]
    public string HandlerType { get; set; }
    [NotNull]
    public string Operation { get; set; }
    public string? PayloadJson { get; set; }          // Сериалізований об'єкт
    public DateTime LastModified { get; set; } = DateTime.UtcNow;

    public bool IsProcessing { get; set; } = false;  // Щоб уникнути повторної обробки під час sync
    public bool IsProcessed { get; set; } = false;

    // Retry and failure lifecycle fields
    public int RetryCount { get; set; } = 0;
    public DateTime? NextRetryAt { get; set; }
    public DateTime? LastRetryAt { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public bool IsFailed { get; set; } = false;
}
