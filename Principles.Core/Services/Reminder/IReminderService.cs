namespace Principles.Core.Services;

public interface IReminderService
{
    bool IsLocalNotificationSupported();
    Task CancelLocallyAsync( int id );
    Task CancelAllLocallyAsync();
    Task ClearDeliveredLocallyAsync();
    Task SaveLocallyAsync( int id, string title, string description, DateTime notifyTime, ReminderRepeat repeatType );
    Task RenamePendingNotificationTitlesAsync( string oldTitle, string newTitle );
    Task TryToRecoverAllUserRemindersAsync();
    Task<Reminder> HabitsReportReminderAsync();
    Task<SaveHabitsReportReminderResponse> SaveHabitsReportReminderAsync( Reminder reminder );
    Task<bool> RequestAccessToSendNotificationsAsync();
    void Cancel( int id );
    Task AddNotificationToDeviceAsync( bool isNewHabit, UserHabitReminder reminder, WeekDay weekDay );
}
