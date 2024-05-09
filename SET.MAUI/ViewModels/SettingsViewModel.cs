using System.Windows.Input;

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
    private async Task LogoutAsync()
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
    private async Task DeleteAccountAsync()
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