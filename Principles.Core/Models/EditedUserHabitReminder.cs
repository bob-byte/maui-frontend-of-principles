using System;
using System.Collections.Generic;

namespace Principles.Core.Models;

public class EditedUserHabitReminder
{
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime Time { get; set; }
    public bool IsEnabled { get; set; }
    public IList<WeekDay> DaysOfWeek { get; set; }
}
