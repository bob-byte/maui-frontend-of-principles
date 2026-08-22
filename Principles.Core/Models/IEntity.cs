namespace Principles.Core.Models;

public interface IEntity : IOfflineEntity
{
    public long Id { get; set; }
    public DateTime LastModified { get; set; }
}
