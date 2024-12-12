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
    public ChangePasswordService( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
        
    }

    public Task ChangePasswordAsync( string email, string newPassword )
    {
        string firstKey = "yX7g53NL7X)xjV7#6DP+ipK5n)@9)_r!";
        string secondKey = "M%m5Vy9R(_k74t^M";

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
