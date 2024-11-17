using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Principles.Core.Models;

public class SaveHabitResponse
{
    public long Id { get; set; }
    public long FrequencyId { get; set; }
    public List<Reminder>? ReminderIds { get; set;}

    public class Reminder
    {
        public long Id { get; set; }
        public List<WeekDay>? DaysOfWeek { get; set; }
    }

    public class WeekDay
    {
        public long Id { get; set; }
        public int NotificationRequestId { get; set; }
        public DayOfWeek Type { get; set; }
    }
}
