namespace Principles.ViewModels;

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

    public ForgetPasswordViewModel( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
        ChangePasswordService = serviceProvider.GetRequiredService<IChangePasswordService>();
        LoginService = serviceProvider.GetRequiredService<ILoginService>();
    }

    public IChangePasswordService ChangePasswordService { get; }

    public ILoginService LoginService { get; }

    private void ApplyQuery( IDictionary<string, object> query )
    {
        if (query.TryGetValue( "Email", out object? value ))
        {
            Email = (ValidatableObject<string>)value;
        }

        NewPassword = new ValidatableObject<string>();
        NewPassword.Validations.Add( new IsNotNullOrWhiteSpaceRule() );
        NewPassword.Validations.Add( new NewPasswordRule() );
    }

    public override async Task InitializePopupAsync( IDictionary<string, object> query )
    {
        ApplyQuery( query );
        await base.InitializePopupAsync( query );
    }

    [RelayCommand]
    private async Task ChangePasswordAsync()
    {
        ValidateEmail();
        ValidateNewPassword();

        if (Email.IsValid && NewPassword.IsValid)
        {
            await UiBusyFor( async () =>
            {
                await ChangePasswordService.SendCodeAsync( Email.Value );

                Dictionary<string, object> parameters = new()
                {
                    { "NewPassword", NewPassword.Value },
                    { "Email", Email.Value },
                };
                await DialogService.ShowPopupAsync<ConfirmEmailPopupViewModel>( parameters );
            } );
        }
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
