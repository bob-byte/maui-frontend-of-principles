
using Plugin.LocalNotification;

namespace Principles.ViewModels;

public partial class StartupViewModel : BaseViewModel
{
    private readonly IGoogleAuthService m_googleAuthService;

    [ObservableProperty]
    private ObservableCollectionEx<AppFeature> m_appFeatures;

    public StartupViewModel( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
        m_googleAuthService = serviceProvider.GetRequiredService<IGoogleAuthService>();

        m_appFeatures = new ObservableCollectionEx<AppFeature>
        {
            new() {  Title = LocStrings.TransformAreasOfLifeTitle, Description =  LocStrings.TransformAreasOfLifeDescription},
            new() {  Title = LocStrings.ChatWithHelperTitle, Description = LocStrings.ChatWithHelperDescription },
            new() {  Title = LocStrings.GroupHabitsByGoalsTitle, Description = LocStrings.GroupHabitsByGoalsDescription },
            new() {  Title = LocStrings.GetRecommendationsByAITitle, Description = LocStrings.GetRecommendationsByAIDescription },
            new() {  Title = LocStrings.BecomeTruePersonalityTitle, Description = LocStrings.BecomeTruePersonalityDescription }
        };
    }

    [RelayCommand]
    public async Task ContinueWithGoogleAsync()
    {
        try
        {
            await m_googleAuthService.AuthorizeAsync();
            AllRemindersResponse remindersResponse = await LoadAllRemindersAsync();

            if (remindersResponse.GeneralReminders?.Any() == true ||
                remindersResponse.UserHabitReminders?.Any() == true)
            {
                await LocalNotificationCenter.Current.RequestNotificationPermission();
            }

            foreach (Reminder? reminder in remindersResponse.GeneralReminders.Where( r => r.IsEnabled ))
            {
                NotificationRequest notification = new NotificationRequest
                {
                    NotificationId = reminder.UserNotificationRequestId,
                    Title = reminder.Title,
                    Description = reminder.Description,
                    Schedule = new NotificationRequestSchedule
                    {
                        NotifyTime = DateTime.Today.Add( reminder.Time.ToTimeSpan() ),
                        RepeatType = NotificationRepeat.Daily
                    }
                };

                await LocalNotificationCenter.Current.Show( notification );
            }

            foreach (UserHabitReminder? userHabitReminder in remindersResponse.UserHabitReminders.Where( r => r.IsEnabled ))
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

                    NotificationRequest notification = new NotificationRequest
                    {
                        NotificationId = weekDay.UserNotificationRequestId,
                        Title = userHabitReminder.Title,
                        Description = userHabitReminder.Description,
                        Schedule = new NotificationRequestSchedule
                        {
                            NotifyTime = notifyDateTime,
                            RepeatType = NotificationRepeat.Weekly
                        }
                    };

                    await LocalNotificationCenter.Current.Show( notification );
                }

                await Navigation.GoToInitialViewAsync();
            }
        }
        catch (Exception ex)
        {
            if (ex is not TaskCanceledException)
            {
                LoggingService.LogError( ex, ex.Message );
            }
        }
    }

    [RelayCommand]
    public Task OpenLoginViewAsync()
    {
        return Navigation.NavigateToAsync<LoginViewModel>();
    }

    [RelayCommand]
    public Task OpenSignUpViewAsync()
    {
        return Navigation.NavigateToAsync<SignupViewModel>();
    }
}
