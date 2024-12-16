namespace Principles.Core.Models;

public class HabitDeletionResponse
{
    public class NotificationRequest
    {
        public int Id { get; set; }
    }
    
    public List<NotificationRequest> DeletedNotifications { get; set; }
}