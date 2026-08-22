using Principles.Core.Services;

using System;
namespace Principles.Core.Services;

public class LoginService : BaseRemoteService, ILoginService
{
    private readonly IConfiguration m_configuration;
    private readonly IUserService m_userService;

    public LoginService( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
        m_configuration = serviceProvider.GetRequiredService<IConfiguration>();
        m_userService = serviceProvider.GetRequiredService<IUserService>();
    }

    public async Task LoginAsync( string email, string password )
    {
        string firstKey = m_configuration["EncryptionSettings:FirstKey"]!;
        string secondKey = m_configuration["EncryptionSettings:SecondKey"]!;

        string encryptedPassword = PasswordChanger.EncryptNewPassword( password, firstKey, secondKey );

        LoginRequest request = new( email, encryptedPassword );
        LoginResponse loginResponse = await RequestProvider.PostAsync<LoginRequest, LoginResponse>(
            UrlBuilder.Login,
            request
        ).DefaultConfigureAwait();

        await m_userService.ClearLocalDataAsync().ConfigureAwait( false );
        await SettingsService.SetAuthAccessTokenAsync( loginResponse.Token ).DefaultConfigureAwait();
    }
}
