using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SET.Core.Models;
public class EditedUserHabitReminder
{
    public long Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime Time { get; set; }
    public bool IsEnabled { get; set; }
    public long UserHabitId { get; set; }
    public IList<WeekDay> DaysOfWeek { get; set; }
}
