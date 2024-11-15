using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SET.Core.Services;
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

    public Task<DtoWithId> SaveHabitsReportReminderAsync( Reminder reminder )
    {
        string url = $"{UrlBuilder.HabitsReportReminder}/{reminder.Id}";
        return RequestProvider.PostAsync<Reminder, DtoWithId>( url, reminder, SettingsService.AuthAccessToken );
    }

    public async Task<AllRemindersResponse> LoadAllRemindersAsync()
    {
        string url = $"{UrlBuilder.AllReminders}";
        return await RequestProvider.GetAsync<AllRemindersResponse>( url, SettingsService.AuthAccessToken );
    }
}
