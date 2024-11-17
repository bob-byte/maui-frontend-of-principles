using Principles.Core.Models;

namespace Principles.Core.Services;

public interface IReminderService
{
    Task<Reminder> HabitsReportReminderAsync();
    Task<SaveHabitsReportReminderResponse> SaveHabitsReportReminderAsync( Reminder reminder );
    Task<AllRemindersResponse> LoadAllRemindersAsync();
}