
namespace SET.MAUI.ViewModels;

public partial class SettingsViewModel : BaseViewModel
{
    public SettingsViewModel( IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
        Title = LocStrings.Settings;
        AccountService = serviceProvider.GetRequiredService<IAccountService>();
    }

    public IAccountService AccountService { get; }

    [RelayCommand]
    public Task ShowUserAgreementAsync()
    {
        return Navigation.NavigateToAsync<UserAgreementViewModel>();
    }

    [RelayCommand]
    public Task ShowPrivacyPolicyAsync()
    {
        return Navigation.NavigateToAsync<PrivacyPolicyViewModel>();
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
            LoggingService.LogCriticalError( message: "Cannot open app to send an email", ex );
            await DialogService.ShowErrorAsync( LocStrings.MsgWhenCannotOpenAppToSendEmail ).DefaultConfigureAwait();
        }
    }

    [RelayCommand]
    public async Task LogoutAsync()
    {
        bool doLogout = await DialogService.ShowConfirmAsync( msg: LocStrings.MessageInLogoutConfirm, title: $"{LocStrings.LogoutQuestion}" );
        if (doLogout)
        {
            Logout();

            await Navigation.GoToInitialViewAsync();
        }
    }

    private void Logout()
    {
        SettingsService.AuthAccessToken = string.Empty;
        SettingsService.UserId = string.Empty;

        if (UserName != null)
        {
            UserName.Value = string.Empty;
        }

        MainSlogan = string.Empty;
        Mission = string.Empty;
        Gender = Gender.Man;
        UserIcon = null;

        ReferenceMessenger.Send( new UserLoggedOutMessage() );
    }

    [RelayCommand]
    public async Task DeleteAccountAsync()
    {
        bool doDelete = await DialogService.ShowConfirmAsync( msg: LocStrings.MessageInDeleteAccountConfirm, title: $"{LocStrings.DeleteAccountQuestion}" );
        if (doDelete)
        {
            await UiBusyFor( async () =>
            {
                await AccountService.DeleteAccountAsync();

                Logout();
                await Navigation.GoToInitialViewAsync();
            } );
        }
    }
}