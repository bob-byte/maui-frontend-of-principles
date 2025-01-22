using Microsoft.Maui.Controls;
using Plugin.LocalNotification;
using System;
using System.Net.Mime;
using System.Text;
using System.Text.Json.Serialization;

namespace Principles.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
    public LocalizationResourceManager LocalizationResourceManager
        => LocalizationResourceManager.Instance;
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

        Title = LocStrings.Login;
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
                    await ReminderService.TryToRecoverAllUserRemindersAsync();
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
        Email.Validations.Add( new IsNotNullOrWhiteSpaceRule{ ValidationMessage = (string)LocalizationResourceManager.Instance["RequiredErrorText"] } );
        Email.Validations.Add( new EmailRule{ ValidationMessage = LocStrings.EmailMustHaveCorrectValue } );

        Password.Validations.Add( new IsNotNullOrWhiteSpaceRule{ ValidationMessage = LocStrings.RequiredErrorText } );
    }
}
