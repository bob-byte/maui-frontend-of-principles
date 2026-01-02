
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;

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

    private int m_validConfirmationCode;

    private ConfirmEmailPopupViewModel m_confirmEmailPopupViewModel;
    private ConfirmEmailPopup m_confirmEmailPopup;
    public ForgetPasswordViewModel( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
        ChangePasswordService = serviceProvider.GetRequiredService<IChangePasswordService>();
        LoginService = serviceProvider.GetRequiredService<ILoginService>();
        m_confirmEmailPopupViewModel = new ConfirmEmailPopupViewModel( serviceProvider );
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

        if(Email.IsValid && NewPassword.IsValid)
        {
            await UiBusyFor( async () =>
            {
                m_validConfirmationCode = await ChangePasswordService.GeneratedCodeAsync( Email.Value );

                Dictionary<string, object> parameters = new()
                    {
                        { "NewPassword", NewPassword.Value },
                        { "ConfirmationCode", m_validConfirmationCode },
                        { "Email", Email }
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