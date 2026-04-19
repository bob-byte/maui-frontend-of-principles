using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Principles.Core.Models;

[Table( "UserHabitReminder" )]
public class UserHabitReminder : ICloneable, IEntity
{
    [PrimaryKey, AutoIncrement]
    public long LocalId { get; set; }
    public long Id { get; set; }
    public DateTime LastModified { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public TimeOnly Time { get; set; }
    public bool IsEnabled { get; set; }
    public long UserHabitLocalId { get; set; }
    [Ignore]
    public IList<WeekDay>? DaysOfWeek { get; set; }

    public object Clone()
    {
        var clone = MemberwiseClone() as UserHabitReminder;
        clone!.DaysOfWeek = new List<WeekDay>();
        foreach (WeekDay day in DaysOfWeek ?? [])
        {
            clone.DaysOfWeek.Add((day.Clone() as WeekDay)!);
        }

        return clone;
    }
}
