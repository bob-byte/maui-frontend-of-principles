using CommunityToolkit.Maui.Views;

namespace Principles.ViewModels;

public partial class ChangePasswordViewModel : BaseViewModel
{
    [ObservableProperty]
    private ValidatableObject<string> m_newPassword;

    private int m_validConfirmationCode;

    private IChangePasswordService ChangePasswordService { get; }

    public ChangePasswordViewModel( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
        ChangePasswordService = serviceProvider.GetRequiredService<IChangePasswordService>();
    }

    public override async Task InitializeAsync( object? parameter = null )
    {
        await base.InitializeAsync(parameter);
        
        Email = CachingService.GetStoredValue( PreferenceKeys.USER_EMAIL );

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
                Page? currentPage = Application.Current?.Windows[0].Page?.Navigation?.NavigationStack.LastOrDefault();

                if (currentPage is not null)
                {
                    ConfirmEmailPopupViewModel confirmEmailPopupViewModel = ServiceProvider.GetRequiredService<ConfirmEmailPopupViewModel>();
                
                    confirmEmailPopupViewModel.SetData( NewPassword.Value, m_validConfirmationCode, Email );
                    ConfirmEmailPopup confirmEmailPopup = new( confirmEmailPopupViewModel );

                    currentPage.ShowPopup( confirmEmailPopup );
                    
                    NewPassword = new ValidatableObject<string>();
                    NewPassword.Validations.Add( new IsNotNullOrWhiteSpaceRule() );
                    NewPassword.Validations.Add( new NewPasswordRule() );
                }
            } );
        }
    }
}