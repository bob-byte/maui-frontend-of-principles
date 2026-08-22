using System.Security.Authentication;

namespace Principles.Core.Services;

public class AppleAuthService : IAppleAuthService
{
    private readonly IRequestProvider m_requestProvider;
    private readonly ISettingsService m_settingsService;
    private readonly IUrlBuilder m_urlBuilder;
    private readonly ILoggingService m_loggingService;
    private readonly IUserService m_userService;

    public AppleAuthService( IServiceProvider serviceProvider )
    {
        m_requestProvider = serviceProvider.GetRequiredService<IRequestProvider>();
        m_settingsService = serviceProvider.GetRequiredService<ISettingsService>();
        m_urlBuilder = serviceProvider.GetRequiredService<IUrlBuilder>();
        m_loggingService = serviceProvider.GetRequiredService<ILoggingService>();
        m_userService = serviceProvider.GetRequiredService<IUserService>();
    }

    public async Task AuthorizeAsync()
    {
        if (DeviceInfo.Platform == DevicePlatform.iOS && DeviceInfo.Version.Major >= 13)
        {
            var options = new AppleSignInAuthenticator.Options
            {
                IncludeEmailScope = true,
                IncludeFullNameScope = true,
            };
            WebAuthenticatorResult webAuthResult = await AppleSignInAuthenticator.AuthenticateAsync( options );

            if (string.IsNullOrWhiteSpace( webAuthResult.IdToken ))
            {
                m_loggingService.LogError(
                    "Failed to authenticate using apple, because ID token could not be retrieved." );
                throw new AuthenticationException( "Failed to authenticate" );
            }

            AppleAuthRequest request = new();
            request.IdToken = webAuthResult.IdToken;
            LoginResponse loginResponse = await m_requestProvider
                .PostAsync<AppleAuthRequest, LoginResponse>( m_urlBuilder.AppleAuth, request ).DefaultConfigureAwait();

            await m_userService.ClearLocalDataAsync().ConfigureAwait( false );
            await m_settingsService.SetAuthAccessTokenAsync( loginResponse.Token ).DefaultConfigureAwait();
        }
        else
        {
            string encodedResponseType = Uri.EscapeDataString( "code id_token" );
            string encodedScope = Uri.EscapeDataString( "openid name email" );
            string responseMode = Uri.EscapeDataString( "form_post" );

            string _authUrl = "https://appleid.apple.com/auth/authorize";
            string _clientId = "com.set.principles.auth";
            string _redirectUri = "https://principles.top/api/auth/apple";

            Uri authUrl = new(
                $"{_authUrl}?client_id={_clientId}" +
                $"&redirect_uri={_redirectUri}" +
                $"&response_type={encodedResponseType}" +
                $"&scope={encodedScope}" +
                $"&response_mode={responseMode}"
            );

            Uri callbackUrl = new("com.set.principles://");

            WebAuthenticatorResult webAuthResult = await WebAuthenticator.AuthenticateAsync( authUrl, callbackUrl );

            string? appToken = webAuthResult?.Properties.GetValueOrDefault( "app_token" );

            if (appToken is null)
            {
                throw new AuthenticationException(
                    "Failed to authenticate to apple, because app token could not be retrieved."
                );
            }
            else
            {
                await m_userService.ClearLocalDataAsync().ConfigureAwait( false );
                await m_settingsService.SetAuthAccessTokenAsync( appToken );
            }
        }
    }
}
