
using Plugin.LocalNotification;

namespace Principles.Core.Services;

public interface IReminderService
{
    Task SaveLocallyAsync( int id, string title, string description, DateTime notifyTime, ReminderRepeat repeatType );
    Task SaveLocallyAsync( NotificationRequest notification );
    Task TryToRecoverAllUserRemindersAsync();
    Task<Reminder> HabitsReportReminderAsync();
    Task<SaveHabitsReportReminderResponse> SaveHabitsReportReminderAsync( Reminder reminder );
}