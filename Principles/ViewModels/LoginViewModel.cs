using Microsoft.Maui.Controls;
using Plugin.LocalNotification;
using System;
using System.Net.Mime;
using System.Text;
using System.Text.Json.Serialization;

namespace Principles.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
    public LoginViewModel( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
        m_email = new ValidatableObject<string>();
        m_password = new ValidatableObject<string>();
        LoginService = serviceProvider.GetRequiredService<ILoginService>();
        ReminderService = serviceProvider.GetRequiredService<IReminderService>();
        
        AddValidations();

        Title = LocStrings.Login;
    }

    [ObservableProperty]
    private ValidatableObject<string> m_email;

    [ObservableProperty]
    private ValidatableObject<string> m_password;

    private bool IsEmailAndPasswordValid => 
        Email.IsValid && Password.IsValid;

    public ILoginService LoginService { get; }
    public IReminderService ReminderService { get; }
    
    public override Task InitializeAsync( object? parameter = null )
    {
        return base.InitializeAsync( parameter );
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        ValidateEmail();
        ValidatePassword();

        if (IsEmailAndPasswordValid)
        {
            await UiBusyFor( async () =>
            {
                try
                {
                    await LoginService.LoginAsync( Email.Value, Password.Value );

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
                    }
                    
                    await Navigation.GoToInitialViewAsync();
                    
                    Email = new ValidatableObject<string>();
                    Password = new ValidatableObject<string>();
                    AddValidations();
                }
                catch
                {
                    await SettingsService.SetAuthAccessTokenAsync( string.Empty );
                    throw;
                }
            } );
        }
    }

    [RelayCommand]
    private Task SignupAsync()
    {
        return Navigation.NavigateToAsync<SignupViewModel>();
    }

    [RelayCommand]
    private void ValidateEmail()
    {
        Email.Validate();
    }

    [RelayCommand]
    private void ValidatePassword()
    {
        Password.Validate();
    }

    [RelayCommand]
    public Task OpenUserAgreementAsync()
    {
        return BrowserHelper.OpenUrl( "https://principles.top/useragreement" );
    }

    [RelayCommand]
    public Task OpenPrivacyPolicyAsync()
    {
        return BrowserHelper.OpenUrl( "https://principles.top/privacypolicy" );
    }

    [RelayCommand]
    public async Task OpenForgetPasswordAsync()
    {
        Dictionary<string, object> routeParams = new()
        {
            { "Email", Email }
        };
        await Navigation.NavigateToAsync<ForgetPasswordViewModel>( routeParams );
    }

    private void AddValidations()
    {
        Email.Validations.Add( new IsNotNullOrWhiteSpaceRule{ ValidationMessage = LocStrings.RequiredErrorText } );
        Email.Validations.Add( new EmailRule{ ValidationMessage = LocStrings.EmailMustHaveCorrectValue } );

        Password.Validations.Add( new IsNotNullOrWhiteSpaceRule{ ValidationMessage = LocStrings.RequiredErrorText } );
    }
}
