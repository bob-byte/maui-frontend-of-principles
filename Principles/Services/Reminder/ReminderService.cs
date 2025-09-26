
using Plugin.LocalNotification.AndroidOption;

namespace Principles.Services;

public class ReminderService : BaseRemoteService, IReminderService
{
    private readonly IDialogService m_dialogService;
    
    public ReminderService( IServiceProvider serviceProvider ) 
        : base( serviceProvider )
    {
        m_dialogService = serviceProvider.GetRequiredService<IDialogService>();
    }

    public Task<Reminder> HabitsReportReminderAsync()
    {
        string url = $"{UrlBuilder.HabitsReportReminder}";
        return RequestProvider.GetAsync<Reminder>( url, SettingsService.AuthAccessToken );
    }

    public Task<SaveHabitsReportReminderResponse> SaveHabitsReportReminderAsync( Reminder reminder )
    {
        string url = $"{UrlBuilder.HabitsReportReminder}/{reminder.Id}";
        return RequestProvider.PostAsync<Reminder, SaveHabitsReportReminderResponse>( url, reminder, SettingsService.AuthAccessToken );
    }

    public Task RequestAccessToSendNotificationsAsync()
    {
        return LocalNotificationCenter.Current.RequestNotificationPermission();
    }

    public Task CancelLocallyAsync( int id )
    {
        LocalNotificationCenter.Current.Cancel( id );
        return Task.CompletedTask;
    }

    public async Task TryToRecoverAllUserRemindersAsync()
    {
        if (!LocalNotificationCenter.Current.IsSupported)
        {
            await m_dialogService.ShowAlertAsync( 
                LocStrings.DeviceDoesNotSupportNotifications, 
                LocStrings.Error,
                LocStrings.OK 
            ).DefaultConfigureAwait();
            return;
        }
        
        AllRemindersResponse remindersResponse = await LoadAllRemindersAsync();

        if (remindersResponse.GeneralReminders?.Any(r => r.IsEnabled ) == true ||
            remindersResponse.UserHabitReminders?.Any( r => r.IsEnabled ) == true)
        {
            bool areNotificationsEnabled = await LocalNotificationCenter.Current.AreNotificationsEnabled();
            if (!areNotificationsEnabled)
            {
                //TODO: it should have better view
                await m_dialogService.ShowAlertAsync( 
                    LocStrings.AfterLoginWhenUserAccountHaveReminders, 
                    LocStrings.RestoreReminders,
                    LocStrings.OK 
                );
            
                await LocalNotificationCenter.Current.RequestNotificationPermission();
            }
            
            if (remindersResponse.GeneralReminders?.Any() == true)
            {
                foreach (Reminder? reminder in remindersResponse.GeneralReminders.Where( r => r.IsEnabled ))
                {
                    await SaveLocallyAsync( 
                        reminder.UserNotificationRequestId, 
                        reminder.Title, 
                        reminder.Description, 
                        DateTime.Today.Add( reminder.Time.ToTimeSpan() ), 
                        ReminderRepeat.Daily 
                    );
                }
            }

            if (remindersResponse.UserHabitReminders?.Any() == true)
            {
                foreach (UserHabitReminder? userHabitReminder in remindersResponse.UserHabitReminders.Where( r =>
                             r.IsEnabled ))
                {
                    foreach (WeekDay weekDay in userHabitReminder.DaysOfWeek)
                    {
                        DateTime currentDate = DateTime.Now;
                        TimeSpan currentTime = currentDate.TimeOfDay;

                        int reminderDayIndex = (int)weekDay.Type;
                        int currentDayIndex = (int)currentDate.DayOfWeek;

                        int daysUntilNextReminder = (reminderDayIndex - currentDayIndex + 7) % 7;

                        if (daysUntilNextReminder == 0 && userHabitReminder.Time.ToTimeSpan() < currentTime)
                        {
                            daysUntilNextReminder = 7;
                        }

                        DateTime notifyDateTime = currentDate.Date
                            .AddDays( daysUntilNextReminder )
                            .Add( userHabitReminder.Time.ToTimeSpan() );
                        
                        await SaveLocallyAsync( 
                            weekDay.UserNotificationRequestId, 
                            userHabitReminder.Title, 
                            userHabitReminder.Description, 
                            notifyDateTime, 
                            ReminderRepeat.Weekly 
                        );
                    }
                }
            }
        }
    }

    public async Task SaveLocallyAsync( int id, string title, string description, DateTime notifyTime, ReminderRepeat repeatType )
    {
        NotificationRepeat notificationRepeat = repeatType switch
        {
            ReminderRepeat.Weekly => NotificationRepeat.Weekly,
            _ => NotificationRepeat.Daily
        };
        
        NotificationRequest notification = new()
        {
            NotificationId = id,
            Title = title,
            Description = description,
            Schedule = new NotificationRequestSchedule
            {
                NotifyTime = notifyTime, 
                RepeatType = notificationRepeat
            },
            Android = new AndroidOptions()
            {
                IconLargeName = new AndroidIcon("logolargesize"),
                IconSmallName = new AndroidIcon("logolargesize"),
            },
        };
        
        await SaveLocallyAsync( notification );
    }

    public async Task SaveLocallyAsync( NotificationRequest notification )
    {
#if IOS
        LocalNotificationCenter.Current.Cancel( notification.NotificationId );
        await LocalNotificationCenter.Current.Show( notification ).DefaultConfigureAwait();
#else
        await LocalNotificationCenter.Current.Show( notification ).DefaultConfigureAwait();
#endif
    }

    private async Task<AllRemindersResponse> LoadAllRemindersAsync()
    {
        string url = $"{UrlBuilder.AllReminders}";
        return await RequestProvider.GetAsync<AllRemindersResponse>( url, SettingsService.AuthAccessToken );
    }
}