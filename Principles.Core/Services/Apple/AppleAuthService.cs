using System.Security.Authentication;

namespace Principles.Core.Services;

public class AppleAuthService : IAppleAuthService
{
    private readonly IRequestProvider m_requestProvider;
    private readonly ISettingsService m_settingsService;
    private readonly IUrlBuilder m_urlBuilder;
    private readonly ILoggingService m_loggingService;

    public AppleAuthService( IServiceProvider serviceProvider )
    {
        m_requestProvider = serviceProvider.GetRequiredService<IRequestProvider>();
        m_settingsService = serviceProvider.GetRequiredService<ISettingsService>();
        m_urlBuilder = serviceProvider.GetRequiredService<IUrlBuilder>();
        m_loggingService = serviceProvider.GetRequiredService<ILoggingService>();
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

            await m_settingsService.SetAuthAccessTokenAsync( loginResponse.Token ).DefaultConfigureAwait();
        }
        else
        {
            throw new NotSupportedException( message: "Apple Sign-In is only supported on iOS 13 or later." );
        }
    }
}