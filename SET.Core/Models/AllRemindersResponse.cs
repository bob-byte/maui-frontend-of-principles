using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SET.Core.Models;
public class AllRemindersResponse
{
    public List<Reminder> RemindersReport { get; set; }
    public List<UserHabitReminder> UserHabitReminders { get; set; }
}
