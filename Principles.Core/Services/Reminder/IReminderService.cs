
namespace Principles.Core.Services;

public interface IReminderService
{
    Task AddAsync( int id, string title, string description, DateTime notifyTime, ReminderRepeat repeatType );
    Task TryToRecoverAllUserRemindersAsync();
    Task<Reminder> HabitsReportReminderAsync();
    Task<SaveHabitsReportReminderResponse> SaveHabitsReportReminderAsync( Reminder reminder );
}