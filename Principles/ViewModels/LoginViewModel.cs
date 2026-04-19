using Microsoft.Maui.Controls;
using Plugin.LocalNotification;

using Principles.Core.Models;
using Principles.Exceptions;

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
        ReferenceMessenger.Register<NewCultureMessage>( this, ( sender, msg ) =>
        {
            ResetValidation();
        } );
    }

    private void ResetValidation()
    {
        Email.Validations.Clear();
        Password.Validations.Clear();
        AddValidations();
    }

    [ObservableProperty]
    private ValidatableObject<string> m_email;

    [ObservableProperty]
    private ValidatableObject<string> m_password;

    [ObservableProperty]
    private bool m_isLoginEnable = true;

    [ObservableProperty]
    private int m_failedAttempts;

    [ObservableProperty]
    private bool m_isTimerVisible;

    [ObservableProperty]
    private string m_timerMessage;

    private bool IsEmailAndPasswordValid =>
        Email.IsValid && Password.IsValid;

    public ILoginService LoginService { get; }
    public IReminderService ReminderService { get; }

    [RelayCommand( CanExecute = nameof( CanLogin ) )]
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
                }
                catch (ExtendedHttpRequestException ex)
                {
                    await SettingsService.SetAuthAccessTokenAsync( string.Empty );

                    if (ex.Message == "InvalidEmailOrPassword")
                    {
                        FailedAttempts++;
                        if (FailedAttempts >= 5)
                        {
                            StartLockoutTimer();
                        }
                    }

                    throw;
                }

                try
                {
                    await ReminderService.TryToRecoverAllUserRemindersAsync();
                }
                catch
                {
                    await SettingsService.SetAuthAccessTokenAsync( string.Empty );
                    throw;
                }

                await Navigation.GoToInitialViewAsync( SyncTrigger.AuthCompleted );

                Email = new ValidatableObject<string>();
                Password = new ValidatableObject<string>();
                AddValidations();
            } );
        }
    }

    private bool CanLogin()
    {
        return IsLoginEnable;
    }

    private void StartLockoutTimer()
    {
        IsLoginEnable = false;
        IsTimerVisible = true;
        TimerMessage = string.Empty;

        int lockoutDuration = 30;
        DateTime lockoutStartTime = DateTime.Now;

        Application.Current!.Dispatcher.StartTimer(
            interval: TimeSpan.FromSeconds( 1 ),
            callback: () =>
            {
                double elapsed = (DateTime.Now - lockoutStartTime).TotalSeconds;

                int remainingTime = lockoutDuration - (int)elapsed;

                bool continueTimer;

                if (remainingTime <= 0)
                {
                    IsLoginEnable = true;
                    IsTimerVisible = false;
                    TimerMessage = string.Empty;

                    FailedAttempts--;

                    if (LoginCommand is IAsyncRelayCommand asyncRelayCommand)
                    {
                        asyncRelayCommand.NotifyCanExecuteChanged();
                    }

                    continueTimer = false;
                }
                else
                {
                    TimerMessage = $"{LocStrings.TryAgain} {remainingTime} {LocStrings.SecondsInShort}.";
                    continueTimer = true;
                }

                return continueTimer;
            }
        );
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
        Email.Validations.Add( new IsNotNullOrWhiteSpaceRule { ValidationMessage = LocStrings.RequiredErrorText } );
        Email.Validations.Add( new EmailRule { ValidationMessage = LocStrings.EmailMustHaveCorrectValue } );

        Password.Validations.Add( new IsNotNullOrWhiteSpaceRule { ValidationMessage = LocStrings.RequiredErrorText } );
    }
}
