using Principles.Core.Services;

using System;
namespace Principles.Core.Services;

public class LoginService : BaseRemoteService, ILoginService
{
    public LoginService( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
        
    }

    public async Task LoginAsync( string email, string password )
    {
        string firstKey = "yX7g53NL7X)xjV7#6DP+ipK5n)@9)_r!";
        string secondKey = "M%m5Vy9R(_k74t^M";

        string encryptedPassword = PasswordChanger.EncryptNewPassword( password, firstKey, secondKey );

        LoginRequest request = new( email, encryptedPassword );
        LoginResponse loginResponse = await RequestProvider.PostAsync<LoginRequest, LoginResponse>(
            UrlBuilder.Login,
            request
        ).DefaultConfigureAwait();

        await SettingsService.SetAuthAccessTokenAsync( loginResponse.Token ).DefaultConfigureAwait();
    }
}
