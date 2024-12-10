using System;

namespace Principles.Core.Models;

public class EditedReminderReport
{
    public long Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public TimeSpan Time { get; set; }
    public bool IsEnabled { get; set; }
    public int UserNotificationRequestId { get; set; }
}
