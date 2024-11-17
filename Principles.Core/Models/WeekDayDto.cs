using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Principles.Core.Models;

public class WeekDay
{
    public long Id { get; set; }
    public DayOfWeek Type { get; set; }
    public int UserNotificationRequestId { get; set; }
}
