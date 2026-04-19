using System;

namespace Principles.Core.Models;

[Table( "Reminder" )]
public class Reminder : IEntity
{
    [PrimaryKey, AutoIncrement]
    public long LocalId { get; set; }
    public long Id { get; set; }
    public DateTime LastModified { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public TimeOnly Time { get; set; }
    public bool IsEnabled { get; set; }
    public int UserNotificationRequestId { get; set; }
}
