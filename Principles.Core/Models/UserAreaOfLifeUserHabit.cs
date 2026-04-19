namespace Principles.Core.Models;

[Table( "UserAreaOfLifeUserHabit" )]
public class UserAreaOfLifeUserHabit : IOfflineEntity
{
    [PrimaryKey, AutoIncrement]
    public long LocalId { get; set; }
    public long UserAreaOfLifeLocalId { get; set; }
    public long UserHabitLocalId { get; set; }
}
