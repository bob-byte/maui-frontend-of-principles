using System;
using System.Collections.Generic;

namespace Principles.Core.Models;

public class EditedUserHabitReminder
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Time { get; set; } = DateTime.Today.AddHours( 8 );
    public bool IsEnabled { get; set; } = true;
    public IList<WeekDay> DaysOfWeek { get; set; } = new List<WeekDay>();
}
