using Plugin.LocalNotification;

namespace Principles.ViewModels;

public partial class SettingsViewModel : BaseViewModel
{
    public SettingsViewModel( IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
        UserService = serviceProvider.GetRequiredService<IUserService>();
    }

    public IUserService UserService { get; }

    [RelayCommand]
    public Task ShowUserAgreementAsync()
    {
        return BrowserHelper.OpenUrl( "https://principles.top/useragreement" );
    }

    [RelayCommand]
    public Task ShowPrivacyPolicyAsync()
    {
        return BrowserHelper.OpenUrl( "https://principles.top/privacypolicy" );
    }

    [RelayCommand]
    public Task ShowTelegramChannelAsync()
    {
        return BrowserHelper.OpenUrl( "https://t.me/principles_app" );
    }

    [RelayCommand]
    public async Task OpenEmailAsync()
    {
        try
        {
            string email = "app@principles.top";

            var uri = new Uri( $"mailto:{email}" );

            bool isAppOpened = await Launcher.OpenAsync( uri );
            if (!isAppOpened)
            {
                await DialogService.ShowErrorAsync( LocStrings.MsgWhenCannotOpenAppToSendEmail );
            }
        }
        catch (Exception ex)
        {
            LoggingService.LogCriticalError( ex, message: "Cannot open app to send an email" );
            await DialogService.ShowErrorAsync( LocStrings.MsgWhenCannotOpenAppToSendEmail ).DefaultConfigureAwait();
        }
    }

    [RelayCommand]
    public async Task LogoutAsync()
    {
        bool doLogout = await DialogService.ShowConfirmAsync( msg: LocStrings.MessageInLogoutConfirm, title: $"{LocStrings.LogoutQuestion}" );
        if (doLogout)
        {
            await base.LogoutAsync().DefaultConfigureAwait();
        }
    }

    [RelayCommand]
    public async Task DeleteAccountAsync()
    {
        bool doDelete = await DialogService.ShowConfirmAsync( msg: LocStrings.MessageInDeleteAccountConfirm, title: $"{LocStrings.DeleteAccountQuestion}" );
        if (doDelete)
        {
            await UiBusyFor( async () =>
            {
                await UserService.DeleteAccountAsync();

                await base.LogoutAsync().DefaultConfigureAwait();
            } );
        }
    }

    [RelayCommand]
    private void ChangeLanguage(string? selectedLanguageCode)
    {
        if (selectedLanguageCode is not null)
        {
            CultureInfo newCulture = new( selectedLanguageCode );
            LocalizationResourceManager.Initialize( SettingsService );
            LocalizationResourceManager.Instance.SetCulture( newCulture );
            ReferenceMessenger.Send( new NewCultureMessage( newCulture ) );
        }
    }

    [RelayCommand]
    private Task ShowChangePasswordAsync()
    {
        return Navigation.NavigateToAsync<ChangePasswordViewModel>();
    }
}
