using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Principles.Core.Models;

public class WeekDay : IEntity, ICloneable
{
    public long Id { get; set; }
    public DateTime LastModified { get; set; }
    public long LocalId { get; set; }
    public DayOfWeek Type { get; set; }
    public int UserNotificationRequestId { get; set; }
    public long UserHabitReminderId { get; set; }

    public object Clone()
    {
        return MemberwiseClone();
    }
}
