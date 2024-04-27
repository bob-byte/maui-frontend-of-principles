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