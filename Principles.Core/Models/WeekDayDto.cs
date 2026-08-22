using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Principles.Core.Models;

[Table( "WeekDay" )]
public class WeekDay : IEntity, ICloneable
{
    public long Id { get; set; }
    public DateTime LastModified { get; set; }
    [PrimaryKey, AutoIncrement]
    public long LocalId { get; set; }
    public DayOfWeek Type { get; set; }
    public int UserNotificationRequestId { get; set; }
    public long UserHabitReminderLocalId { get; set; }

    public object Clone()
    {
        return MemberwiseClone();
    }
}
