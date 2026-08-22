using CommunityToolkit.Maui.Extensions;
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

    public override async Task InitializePageAsync( IDictionary<string, object> query )
    {
        await base.InitializePageAsync( query );
        await InitUserInfoAsync();

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
                Page? currentPage = Shell.Current?.CurrentPage;

                if (currentPage is not null)
                {
                    Dictionary<string, object> popupParameters = new()
                    {
                        ["NewPassword"] = NewPassword.Value,
                        ["ConfirmationCode"] = m_validConfirmationCode,
                        ["Email"] = Email
                    };
                    await DialogService.ShowPopupAsync<ConfirmEmailPopupViewModel>( popupParameters );

                    NewPassword = new ValidatableObject<string>();
                    NewPassword.Validations.Add( new IsNotNullOrWhiteSpaceRule() );
                    NewPassword.Validations.Add( new NewPasswordRule() );
                }
            } );
        }
    }
}
