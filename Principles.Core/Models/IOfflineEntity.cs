namespace Principles.Core.Models;

public interface IOfflineEntity
{
    [PrimaryKey, AutoIncrement]
    public long LocalId { get; set; }
}