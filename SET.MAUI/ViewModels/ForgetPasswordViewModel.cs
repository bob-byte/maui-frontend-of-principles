
namespace SET.MAUI.ViewModels;

public partial class ForgetPasswordViewModel : BaseViewModel
{
    [ObservableProperty]
    private bool m_isConfirmChangePasswordOpen;

    [ObservableProperty]
    private ValidatableObject<string> m_email;

    [ObservableProperty]
    private ValidatableObject<string> m_newPassword;

    [ObservableProperty]
    private string m_confirmationCodeByUser;

    private int m_validConfirmationCode;

    public ForgetPasswordViewModel( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
        ChangePasswordService = serviceProvider.GetRequiredService<IChangePasswordService>();
        LoginService = serviceProvider.GetRequiredService<ILoginService>();
    }

    public IChangePasswordService ChangePasswordService { get; }

    public ILoginService LoginService { get; }

    public override void ApplyQueryAttributes( IDictionary<string, object> query )
    {
        base.ApplyQueryAttributes( query );

        if (query.TryGetValue( "Email", out object? value ))
        {
            Email = (ValidatableObject<string>)value;
        }

        NewPassword = new ValidatableObject<string>();
        NewPassword.Validations.Add( new IsNotNullOrWhiteSpaceRule() );
        NewPassword.Validations.Add( new NewPasswordRule() );
    }

    [RelayCommand]
    private async Task ChangePasswordAsync()
    {
        ValidateEmail();
        ValidateNewPassword();

        if(Email.IsValid && NewPassword.IsValid)
        {
            await UiBusyFor( async () =>
            {
                m_validConfirmationCode = await ChangePasswordService.GeneratedCodeAsync( Email.Value );
                IsConfirmChangePasswordOpen = true;
            } );
        }
    }

    [RelayCommand]
    private async Task ConfirmPasswordChangeAsync()
    {
        await UiBusyFor( async () =>
        {
            _ = int.TryParse( ConfirmationCodeByUser, out int setCodeByUser );
            if (setCodeByUser == m_validConfirmationCode)
            {
                await ChangePasswordService.ChangePasswordAsync( Email.Value, NewPassword.Value );

                IsConfirmChangePasswordOpen = false;
                await DialogService.ShowAlertAsync( LocStrings.YourPasswordSuccessfullyChanged, LocStrings.Success, LocStrings.OK );

                await LoginService.LoginAsync( Email.Value, NewPassword.Value );
                await Navigation.GoToInitialViewAsync();
            }
            else
            {
                IsConfirmChangePasswordOpen = false;
                await DialogService.ShowErrorAsync( LocStrings.WrongConfirmationCode );
            }
        } );
    }

    [RelayCommand]
    private void ValidateEmail()
    {
        Email.Validate();
    }

    [RelayCommand]
    private void ValidateNewPassword()
    {
        NewPassword.Validate();
    }
}