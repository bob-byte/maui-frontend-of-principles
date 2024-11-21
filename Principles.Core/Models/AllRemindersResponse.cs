using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Principles.Core.Models;
public class AllRemindersResponse
{
    public List<Reminder> GeneralReminders { get; set; }
    public List<UserHabitReminder> UserHabitReminders { get; set; }
}
