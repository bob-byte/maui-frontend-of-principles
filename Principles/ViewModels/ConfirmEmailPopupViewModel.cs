namespace Principles.ViewModels;

public partial class ConfirmEmailPopupViewModel : BaseViewModel
{
    [ObservableProperty]
    private string? m_confirmationCodeByUser;

    private string m_email;
    private string m_password;
    private int m_validConfirmationCode;

    private IChangePasswordService ChangePasswordService { get; }
    
    public ConfirmEmailPopupViewModel( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
        ChangePasswordService = serviceProvider.GetRequiredService<IChangePasswordService>();
    }

    public void SetData( string newPassword, int confirmationCode, string email )
    {
        m_password = newPassword;
        m_validConfirmationCode = confirmationCode;
        m_email = email;
    }

    [RelayCommand]
    private async Task ConfirmPasswordChangeAsync()
    {
        _ = int.TryParse( ConfirmationCodeByUser, out int setCodeByUser );

        if (setCodeByUser == m_validConfirmationCode)
        {
            ConfirmationCodeByUser = string.Empty;
            
            bool isSuccessfullyChangedPassword = false;
            
            await UiBusyFor( async () =>
            {
                await ChangePasswordService.ChangePasswordAsync( m_email, m_password );
                isSuccessfullyChangedPassword = true;
            } );

            if (isSuccessfullyChangedPassword)
            {
                await DialogService.ShowAlertAsync(
                    LocStrings.YourPasswordSuccessfullyChanged, 
                    LocStrings.Success,
                    LocStrings.OK
                );

                await Navigation.GoToInitialViewAsync();
            }
        }
        else
        {
            await DialogService.ShowErrorAsync( LocStrings.WrongConfirmationCode );
        }
    }
}