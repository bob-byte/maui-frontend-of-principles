
using Microsoft.Maui.ApplicationModel.Communication;

using System.Net.Mail;
using System.Net;
using SET.Core.Services.ChangePassword;

namespace SET.MAUI.ViewModels;

public partial class ForgetPasswordViewModel : BaseViewModel
{
    [ObservableProperty]
    private bool m_isConfirmChangePasswordOpen;

    [ObservableProperty]
    private ValidatableObject<string> m_email;

    [ObservableProperty]
    private string m_newPassword;

    [ObservableProperty]
    private string m_confirmationCodeByUser;

    public ForgetPasswordViewModel( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
        ChangePasswordService = serviceProvider.GetRequiredService<IChangePasswordService>();
    }

    public override void ApplyQueryAttributes( IDictionary<string, object> query )
    {
        base.ApplyQueryAttributes( query );

        if (query.TryGetValue( "Email", out object? value ))
        {
            Email = (ValidatableObject<string>)value;
        }
    }
    public IChangePasswordService ChangePasswordService { get; set; }
    [RelayCommand]
    private void ValidateEmail()
    {
        Email.Validate();
    }

    [RelayCommand]
    private async void ChangePassword()
    {
        string userEmail = Email.Value;

        if (!string.IsNullOrEmpty( userEmail ) && !string.IsNullOrEmpty( NewPassword ))
        {
            await ChangePasswordService.SendEmailAsync( userEmail );
            IsConfirmChangePasswordOpen = true;
        }
        else
        {
            await DialogService.ShowErrorAsync( LocStrings.EmailAndPasswordFieldsAreEmpty );
        }
    }

    [RelayCommand]
    private async Task ConfirmPasswordChange()
    {
        string sendedConfirmationCode = await ChangePasswordService.GetConfirmationCodeAsync();

        if (ConfirmationCodeByUser == sendedConfirmationCode)
        {
            await ChangePasswordService.ChangePasswordAsync( Email.Value, NewPassword );
        }
        else
        {
            IsConfirmChangePasswordOpen = false;
            await DialogService.ShowErrorAsync( LocStrings.WrongConfirmationCode );
        }
    }
}