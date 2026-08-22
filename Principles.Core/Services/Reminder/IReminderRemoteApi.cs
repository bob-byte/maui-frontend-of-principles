namespace Principles.Core.Services;

public interface IReminderRemoteApi
{
    Task<Reminder> GetHabitsReportReminderAsync();
    Task<SaveHabitsReportReminderResponse> SaveHabitsReportReminderAsync( Reminder reminder );
}
