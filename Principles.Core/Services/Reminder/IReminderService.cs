
namespace Principles.Core.Services;

public interface IReminderService
{
    Task TryToRecoverAllUserRemindersAsync();
    Task<Reminder> HabitsReportReminderAsync();
    Task<SaveHabitsReportReminderResponse> SaveHabitsReportReminderAsync( Reminder reminder );
}