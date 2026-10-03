using Microsoft.Extensions.Configuration;

using Principles.Core.Helpers;

namespace Principles.Core.Services;

public class ChangePasswordService : BaseRemoteService, IChangePasswordService
{
    private readonly IConfiguration m_configuration;

    public ChangePasswordService( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
        m_configuration = serviceProvider.GetRequiredService<IConfiguration>();
    }

    public Task ChangePasswordAsync( string email, string newPassword, int code )
    {
        string firstKey = m_configuration["EncryptionSettings:FirstKey"]!;
        string secondKey = m_configuration["EncryptionSettings:SecondKey"]!;

        string hashedPassword = PasswordChanger.EncryptNewPassword( newPassword, firstKey, secondKey );
        ChangePasswordRequest request = new()
        {
            Email = email,
            NewPassword = hashedPassword,
            Code = code,
        };

        return RequestProvider.PutAsync(
            UrlBuilder.Password,
            request
        );
    }

    public Task SendCodeAsync( string emailWhereSendCode )
    {
        string url = $"{UrlBuilder.CodeGeneration}/?emailWhereSendCode={emailWhereSendCode}";
        return RequestProvider.GetAsync<object>( url );
    }
}
