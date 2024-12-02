
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

    public async Task TryToRecoverAllUserRemindersAsync()
    {
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
                    NotificationRequest notification = new()
                    {
                        NotificationId = reminder.UserNotificationRequestId,
                        Title = reminder.Title,
                        Description = reminder.Description,
                        Schedule = new NotificationRequestSchedule
                        {
                            NotifyTime = DateTime.Today.Add( reminder.Time.ToTimeSpan() ),
#if ANDROID
                            NotifyRepeatInterval = TimeSpan.FromHours( 24 ),
                            RepeatType = NotificationRepeat.TimeInterval
#else
                            RepeatType = NotificationRepeat.Daily
#endif                        
                        }
                    };

                    await LocalNotificationCenter.Current.Show( notification );
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

                        NotificationRequest notification = new()
                        {
                            NotificationId = weekDay.UserNotificationRequestId,
                            Title = userHabitReminder.Title,
                            Description = userHabitReminder.Description,
                            Schedule = new NotificationRequestSchedule
                            {
                                NotifyTime = notifyDateTime, 
#if ANDROID
                                NotifyRepeatInterval = TimeSpan.FromDays( 7 ),
                                RepeatType = NotificationRepeat.TimeInterval
#else
                                RepeatType = NotificationRepeat.Weekly
#endif
                            }
                        };

                        await LocalNotificationCenter.Current.Show( notification );
                    }
                }
            }
        }
    }

    private async Task<AllRemindersResponse> LoadAllRemindersAsync()
    {
        string url = $"{UrlBuilder.AllReminders}";
        return await RequestProvider.GetAsync<AllRemindersResponse>( url, SettingsService.AuthAccessToken );
    }
}