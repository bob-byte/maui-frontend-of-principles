using CommunityToolkit.Maui.Views;

namespace Principles.ViewModels;

public partial class ChangePasswordViewModel : BaseViewModel
{
    [ObservableProperty]
    private ValidatableObject<string> m_newPassword;

    private ConfirmEmailPopupViewModel m_confirmEmailPopupViewModel;
    private ConfirmEmailPopup m_confirmEmailPopup;
    private int m_validConfirmationCode;

    public ChangePasswordViewModel( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
        m_confirmEmailPopupViewModel = new ConfirmEmailPopupViewModel( serviceProvider );
        ChangePasswordService = serviceProvider.GetRequiredService<IChangePasswordService>();
    }
    public IChangePasswordService ChangePasswordService { get; }

    public override void ApplyQueryAttributes( IDictionary<string, object> query )
    {
        base.ApplyQueryAttributes( query );

        if (query.TryGetValue( "Email", out object? value ))
        {
            Email = (string)value;
        }

        NewPassword = new ValidatableObject<string>();
        NewPassword.Validations.Add( new IsNotNullOrWhiteSpaceRule() );
        NewPassword.Validations.Add( new NewPasswordRule() );
    }

    [RelayCommand]
    private void ValidateNewPassword()
    {
        NewPassword.Validate();
    }

    [RelayCommand]
    private async Task OpenConfirmEmailPopupAsync()
    {
        ValidateNewPassword();

        if (NewPassword.IsValid)
        {
            await UiBusyFor( async () =>
            {
                m_validConfirmationCode = await ChangePasswordService.GeneratedCodeAsync( Email );
                Page? currentPage = Application.Current.MainPage.Navigation?.NavigationStack.LastOrDefault();

                m_confirmEmailPopupViewModel.SetData( NewPassword.Value, m_validConfirmationCode, Email );
                m_confirmEmailPopup = new ConfirmEmailPopup( m_confirmEmailPopupViewModel );

                currentPage.ShowPopup( m_confirmEmailPopup );
                NewPassword = new ValidatableObject<string>();
            } );
        }
    }
}