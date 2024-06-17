using Microsoft.Maui.Controls;

using System;
using System.Net.Mime;
using System.Text;
using System.Text.Json.Serialization;

namespace SET.MAUI.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
    public LoginViewModel( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
        m_email = new ValidatableObject<string>();
        m_password = new ValidatableObject<string>();
        LoginService = serviceProvider.GetRequiredService<ILoginService>();
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
                    LoginResponse loginResponse = await LoginService.LoginAsync( Email.Value, Password.Value );

                        //TODO: Change type of SettingsService.UserId to long
                        SettingsService.AuthAccessToken = loginResponse.Token;
                        SettingsService.UserId = loginResponse.UserId.ToString();

                    bool isLoggedIn = !string.IsNullOrWhiteSpace( loginResponse.Token );
                    if (isLoggedIn)
                    {
                        await Navigation.GoToInitialViewAsync();

                        Email = new ValidatableObject<string>();
                        Password = new ValidatableObject<string>();
                        AddValidations();
                    }
                    else
                    {
                        LoggingService.LogFatal( loginResponse.Message );
                        await DialogService.ShowErrorAsync( loginResponse.Message );
                    }
                }
                catch
                {
                    SettingsService.AuthAccessToken = string.Empty;
                    SettingsService.UserId = string.Empty;
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
        return Navigation.NavigateToAsync<UserAgreementViewModel>();
    }

    [RelayCommand]
    public Task OpenPrivacyPolicyAsync()
    {
        return Navigation.NavigateToAsync<PrivacyPolicyViewModel>();
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
