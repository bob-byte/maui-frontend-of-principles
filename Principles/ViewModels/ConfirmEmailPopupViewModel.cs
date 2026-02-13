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

    public override async Task InitializePopupAsync( IDictionary<string, object> query )
    {
        await base.InitializePopupAsync( query );
        if (query.TryGetValue( "NewPassword", out object? newPasswordObj ) && newPasswordObj is string newPassword)
        {
            m_password = newPassword;
        }
        if (query.TryGetValue( "ConfirmationCode", out object? confirmationCodeObj ) && confirmationCodeObj is int confirmationCode)
        {
            ValidConfirmationCode = confirmationCode;
        }
        if (query.TryGetValue( "Email", out object? emailObj ) && emailObj is string email)
        {
            m_email = email;
        }
    }

    [RelayCommand]
    private async Task ConfirmPasswordChangeAsync()
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
                await DialogService.ClosePopupAsync();

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
            await DialogService.ClosePopupAsync();
            await DialogService.ShowErrorAsync( LocStrings.WrongConfirmationCode );
        }
    }
}