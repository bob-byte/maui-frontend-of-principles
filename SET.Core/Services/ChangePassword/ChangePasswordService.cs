using Microsoft.Extensions.Configuration;

using SET.Core.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SET.Core.Services.ChangePassword;
public class ChangePasswordService : BaseRemoteService, IChangePasswordService
{
    private readonly IConfiguration m_configuration;
    public ChangePasswordService( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
        m_configuration = serviceProvider.GetService<IConfiguration>();
    }

    public Task ChangePasswordAsync( string email, string newPassword )
    {
        string firstKey = m_configuration["EncryptionSettings:FirstKey"];
        string secondKey = m_configuration["EncryptionSettings:SecondKey"];

        string hashedPassword = PasswordChanger.EncryptNewPassword( newPassword, firstKey, secondKey );
        ChangePasswordRequest request = new ()
        {
            Email = email,
            NewPassword = hashedPassword
        };

        return RequestProvider.PutAsync(
            UrlBuilder.Password,
            request
        );
    }

    public Task SendEmailAsync( string emailAddress )
    {
        return RequestProvider.PostAsync(
            UrlBuilder.Email,
            emailAddress
        );
    }

    public async Task<string> GetConfirmationCodeAsync()
    {
        try
        {
            string url = UrlBuilder.Email;
            Task<string> responseTask = RequestProvider.GetAsync<string>( url );

            string confirmationCode = await responseTask;

            return confirmationCode;
        }
        catch (Exception ex)
        {
            throw new Exception( "Unable to retrieve the verification code from the server", ex );
        }
    }
}
