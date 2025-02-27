namespace Principles.ViewModels;

public partial class ConfirmEmailPopupViewModel : BaseViewModel
{
    [ObservableProperty]
    private string m_confirmationCodeByUser;

    private string m_email;
    private string m_password;
    private int m_validConfirmationCode;
    private ConfirmEmailPopup m_changePasswordPopup;
    public ConfirmEmailPopupViewModel( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
        ChangePasswordService = serviceProvider.GetRequiredService<IChangePasswordService>();
        LoginService = serviceProvider.GetRequiredService<ILoginService>();
    }
    public IChangePasswordService ChangePasswordService { get; }

    public ILoginService LoginService { get; }

    public void SetData( string newPassword, int confirmationCode, string email )
    {
        m_password = newPassword;
        m_validConfirmationCode = confirmationCode;
        m_email = email;
    }

    [RelayCommand]
    private async Task ConfirmPasswordChangeAsync()
    {
        await UiBusyFor( async () =>
        {
            _ = int.TryParse( ConfirmationCodeByUser, out int setCodeByUser );
            if (setCodeByUser == m_validConfirmationCode)
            {
                await ChangePasswordService.ChangePasswordAsync( m_email, m_password );
                await DialogService.ShowAlertAsync( LocStrings.YourPasswordSuccessfullyChanged, LocStrings.Success, LocStrings.OK );
                if( string.IsNullOrWhiteSpace(SettingsService.AuthAccessToken))
                {
                    await LoginService.LoginAsync( m_email, m_password );
                    await Navigation.GoToInitialViewAsync();
                }
            }
            else
            {
                await DialogService.ShowErrorAsync( LocStrings.WrongConfirmationCode );
            }
        } );
    }
}