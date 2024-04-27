using SET.Core.Services;

using System;
namespace SET.Core.Services;

public class LoginService : BaseRemoteService, ILoginService
{
    public LoginService( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
        //do nothing
    }

    public async Task<LoginResponse> LoginAsync( string email, string password )
    {
        LoginRequest request = new( email, password );
        LoginResponse loginResponse = await RequestProvider.PostAsync<LoginRequest, LoginResponse>(
            UrlBuilder.Login,
            request
        );

        return loginResponse;
    }
}
