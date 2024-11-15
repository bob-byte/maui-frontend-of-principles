using System;
namespace SET.Core.Models
{
    public class Reminder
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public TimeOnly Time { get; set; }
        public bool IsEnabled { get; set; }
        public int UserNotificationRequestId { get; set; }
    }
}

