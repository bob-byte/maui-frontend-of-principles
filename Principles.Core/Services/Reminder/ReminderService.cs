using Principles.Core.Models;

namespace Principles.Core.Services;

public class ReminderService : BaseRemoteService, IReminderService
{
    public ReminderService( IServiceProvider serviceProvider ) : base( serviceProvider )
    {
    }

    public async Task<Reminder> HabitsReportReminderAsync()
    {
        string url = $"{UrlBuilder.HabitsReportReminder}";
        return await RequestProvider.GetAsync<Reminder>( url, SettingsService.AuthAccessToken );
    }

    public Task<SaveHabitsReportReminderResponse> SaveHabitsReportReminderAsync( Reminder reminder )
    {
        string url = $"{UrlBuilder.HabitsReportReminder}/{reminder.Id}";
        return RequestProvider.PostAsync<Reminder, SaveHabitsReportReminderResponse>( url, reminder, SettingsService.AuthAccessToken );
    }
}