namespace Principles.ViewModels;

public partial class ConfirmEmailPopupViewModel : BaseViewModel
{
    [ObservableProperty]
    private string? m_confirmationCodeByUser;

    private string m_email;
    private string m_password;
    
    public int ValidConfirmationCode { get; private set; }

    private IChangePasswordService ChangePasswordService { get; }
    
    public ConfirmEmailPopupViewModel( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
        ChangePasswordService = serviceProvider.GetRequiredService<IChangePasswordService>();
    }

    public void SetData( string newPassword, int confirmationCode, string email )
    {
        m_password = newPassword;
        ValidConfirmationCode = confirmationCode;
        m_email = email;
    }

    internal async Task ConfirmPasswordChangeAsync( Func<Task> closePopupTask )
    {
        _ = int.TryParse( ConfirmationCodeByUser, out int setCodeByUser );

        if (setCodeByUser == ValidConfirmationCode)
        {
            ConfirmationCodeByUser = string.Empty;

            bool isSuccessfullyChangedPassword = false;

            await UiBusyFor(async () =>
            {
                await ChangePasswordService.ChangePasswordAsync( m_email, m_password );
                isSuccessfullyChangedPassword = true;
            });

            if (isSuccessfullyChangedPassword)
            {
                await closePopupTask();

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
            await closePopupTask();
            await DialogService.ShowErrorAsync( LocStrings.WrongConfirmationCode );
        }
    }
}