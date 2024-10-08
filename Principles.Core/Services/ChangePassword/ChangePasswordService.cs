using Microsoft.Extensions.Configuration;

using Principles.Core.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Principles.Core.Services;

public class ChangePasswordService : BaseRemoteService, IChangePasswordService
{
    private readonly IConfiguration m_configuration;

    public ChangePasswordService( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
        m_configuration = serviceProvider.GetRequiredService<IConfiguration>();
    }

    public Task ChangePasswordAsync( string email, string newPassword )
    {
        string firstKey = m_configuration["EncryptionSettings:FirstKey"]!;
        string secondKey = m_configuration["EncryptionSettings:SecondKey"]!;

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

    public async Task<int> GeneratedCodeAsync( string emailWhereSendCode )
    {
        string url = $"{UrlBuilder.CodeGeneration}/?emailWhereSendCode={emailWhereSendCode}";
        GenerateCodeResponse response = await RequestProvider.GetAsync<GenerateCodeResponse>( url ).DefaultConfigureAwait();
        return response.Code;
    }
}
