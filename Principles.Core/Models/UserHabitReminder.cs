using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Principles.Core.Models;

public class UserHabitReminder : ICloneable, IEntity
{
    public long Id { get; set; }
    public DateTime LastModified { get; set; }
    public long LocalId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public TimeOnly Time { get; set; }
    public bool IsEnabled { get; set; }
    public long UserHabitId { get; set; }
    public IList<WeekDay> DaysOfWeek { get; set; }

    public object Clone()
    {
        var clone = MemberwiseClone() as UserHabitReminder;
        clone!.DaysOfWeek = new List<WeekDay>();
        foreach (WeekDay day in DaysOfWeek)
        {
            clone.DaysOfWeek.Add((day.Clone() as WeekDay)!);
        }
        
        return clone;
    }
}
