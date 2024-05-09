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
        Email = new ValidatableObject<string>();
        Password = new ValidatableObject<string>();
        AddValidations();

        return base.InitializeAsync( parameter );
    }

    [RelayCommand(CanExecute = nameof(IsEmailAndPasswordValid))]
    private async Task LoginAsync()
    {
        if (IsEmailAndPasswordValid)
        {
            await UiBusyFor( async () =>
            {
                try
                {
                    LoginResponse loginResponse = await LoginService.LoginAsync( Email.Value, Password.Value );

                    SettingsService.AuthAccessToken = loginResponse.Token;
                    SettingsService.UserId = loginResponse.UserId;

                    bool isLoggedIn = !string.IsNullOrWhiteSpace( loginResponse.Token );
                    if (isLoggedIn)
                    {
                        await Navigation.GoToInitialViewAsync();
                    }
                    else
                    {
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

    private void AddValidations()
    {
        Email.Validations.Add( new IsNotNullOrWhiteSpaceRule{ ValidationMessage = LocStrings.RequiredErrorText } );
        Email.Validations.Add( new EmailRule{ ValidationMessage = LocStrings.EmailMustHaveCorrectValue } );

        Password.Validations.Add( new IsNotNullOrWhiteSpaceRule{ ValidationMessage = LocStrings.RequiredErrorText } );
    }
}
