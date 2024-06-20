using SET.Core.Services;

using System;
namespace SET.Core.Services;

public class LoginService : BaseRemoteService, ILoginService
{
    private readonly IConfiguration m_configuration;

    public LoginService( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
        m_configuration = serviceProvider.GetRequiredService<IConfiguration>();
    }

    public async Task<LoginResponse> LoginAsync( string email, string password )
    {
        string firstKey = m_configuration["EncryptionSettings:FirstKey"]!;
        string secondKey = m_configuration["EncryptionSettings:SecondKey"]!;

        string encryptedPassword = PasswordChanger.EncryptNewPassword( password, firstKey, secondKey );

        LoginRequest request = new( email, encryptedPassword );
        LoginResponse loginResponse = await RequestProvider.PostAsync<LoginRequest, LoginResponse>(
            UrlBuilder.Login,
            request
        ).DefaultConfigureAwait();

        //TODO: Change type of SettingsService.UserId to long
        SettingsService.AuthAccessToken = loginResponse.Token;
        SettingsService.UserId = loginResponse.UserId.ToString();

        return loginResponse;
    }
}
