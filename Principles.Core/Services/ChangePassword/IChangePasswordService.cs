namespace Principles.Core.Services;

public interface IChangePasswordService
{
    Task ChangePasswordAsync( string email, string newPassword, int code );

    /// <summary>Emails a verification code. The code is not returned by the API.</summary>
    Task SendCodeAsync( string emailWhereSendCode );
}
