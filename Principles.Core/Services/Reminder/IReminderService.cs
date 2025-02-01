
using Plugin.LocalNotification;

namespace Principles.Core.Services;

public interface IReminderService
{
    Task SaveAsync( int id, string title, string description, DateTime notifyTime, ReminderRepeat repeatType );
    Task SaveAsync( NotificationRequest notification );
    Task TryToRecoverAllUserRemindersAsync();
    Task<Reminder> HabitsReportReminderAsync();
    Task<SaveHabitsReportReminderResponse> SaveHabitsReportReminderAsync( Reminder reminder );
}