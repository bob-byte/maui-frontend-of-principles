using System;
namespace Principles.Core.Models
{
    public class Reminder
    {
        public DateTime? Time { get; set; }
        public IList<DateOnly>? DaysOfWeek { get; set; }
    }
}

