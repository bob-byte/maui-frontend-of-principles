using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;

namespace SET.Core.Services;

public class GoogleAuthService : IGoogleAuthService
{
    private readonly IConfiguration m_configuration;
    private readonly IRequestProvider m_requestProvider;
    private readonly ISettingsService m_settingsService;
    private readonly IUrlBuilder m_urlBuilder;

    public GoogleAuthService( IServiceProvider serviceProvider )
    {
        m_configuration = serviceProvider.GetRequiredService<IConfiguration>();
        m_requestProvider = serviceProvider.GetRequiredService<IRequestProvider>();
        m_settingsService = serviceProvider.GetRequiredService<ISettingsService>();
        m_urlBuilder = serviceProvider.GetRequiredService<IUrlBuilder>();
    }

    public async Task AuthorizeAsync( ICodeReceiver codeReceiver, IDataStore dataStore )
    {
        string? clientId = null;
#if ANDROID
        clientId = m_configuration["Google:ClientIds:Android"];
#elif IOS
        clientId = m_configuration["Google:ClientIds:IOS"];
#endif

        if (string.IsNullOrEmpty( clientId ))
        {
            throw new InvalidOperationException( "Client secret is missing." );
        }

        string[] requestedScopes = new[]
        {
            "https://www.googleapis.com/auth/userinfo.profile",
            "https://www.googleapis.com/auth/userinfo.email",
            "https://www.googleapis.com/auth/user.gender.read"
        };
        ClientSecrets secrets = new()
        {
            ClientId = clientId,
            ClientSecret = ""
        };

        UserCredential credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
            secrets,
            requestedScopes,
            "user",
            CancellationToken.None,
            dataStore,
            codeReceiver
        ).DefaultConfigureAwait();

        string url = m_urlBuilder.GoogleAuth;
        GoogleAuthRequest request = new()
        {
            AccessToken = credential.Token.AccessToken,
            IdToken = credential.Token.IdToken
        };
        GoogleAuthResponse response = await m_requestProvider.PostAsync<GoogleAuthRequest, GoogleAuthResponse>( url, request ).DefaultConfigureAwait();

        m_settingsService.AuthAccessToken = response.Token;
        m_settingsService.UserId = response.UserId.ToString();
    }
}

