namespace Principles.Core.Models;

public record User : IEntity
{
    public long LocalId { get; set; }
    public long Id { get; set; }
    public string? Name { get; set; }
    public string? MainSlogan { get; set; }
    public string? Mission { get; set; }
    public string? Email { get; set; }
    public Gender Gender { get; set; }
    public DateTime LastModified { get; set; }
    public bool IsAllDataSyncedOnFirstStart { get; set; }
}
