namespace SET.Core;

public class GlobalSettings
{
    public const string AZURE_TAG = "Azure";
    public const string MOCK_TAG = "Mock";
    public const string DEFAULT_ENDPOINT = "https://habitsmentorset.azurewebsites.net";

    private string m_baseIdentityEndpoint;
    private string m_baseGatewayAppEndpoint;

    private GlobalSettings()
    {
        //for tests
        AuthToken = "INSERT AUTHENTICATION TOKEN";

        BaseIdentityEndpoint = DEFAULT_ENDPOINT;
        BaseGatewayAppEndpoint = DEFAULT_ENDPOINT;
    }

    public static GlobalSettings Instance { get; } = new GlobalSettings();

    public string BaseIdentityEndpoint
    {
        get => m_baseIdentityEndpoint;
        private set
        {
            m_baseIdentityEndpoint = value;
            UpdateEndpoint(m_baseIdentityEndpoint);
        }
    }

    public string BaseGatewayAppEndpoint
    {
        get => m_baseGatewayAppEndpoint;
        set
        {
            m_baseGatewayAppEndpoint = value;
            UpdateGatewayAppEndpoint(m_baseGatewayAppEndpoint);
        }
    }

    public string ClientId { get; } = "xamarin";

    public string ClientSecret { get; } = "secret";

    public string AuthToken { get; set; }

    public string RegisterWebsite { get; set; }

    public string AuthorizeEndpoint { get; set; }

    public string UserInfoEndpoint { get; set; }

    public string TokenEndpoint { get; set; }

    public string LogoutEndpoint { get; set; }

    public string Callback { get; set; }

    public string LogoutCallback { get; set; }

    public string GatewayAppEndpoint { get; set; }

    private void UpdateEndpoint(string endpoint)
    {
        RegisterWebsite = $"{endpoint}/Account/Register";
        LogoutCallback = $"{endpoint}/Account/Redirecting";

        var connectBaseEndpoint = $"{endpoint}/connect";
        //AuthorizeEndpoint = $"{connectBaseEndpoint}/authorize";
        AuthorizeEndpoint = DEFAULT_ENDPOINT;
        UserInfoEndpoint = $"{connectBaseEndpoint}/userinfo";
        TokenEndpoint = $"{connectBaseEndpoint}/token";
        LogoutEndpoint = $"{connectBaseEndpoint}/endsession";

        var baseUri = GlobalSettings.ExtractBaseUri(endpoint);
        Callback = $"{baseUri}/xamarincallback";
    }

    private void UpdateGatewayAppEndpoint(string endpoint)
    {
        GatewayAppEndpoint = endpoint;
    }

    private static string ExtractBaseUri(string endpoint)
    {
        try
        {
            var uri = new Uri(endpoint);
            string baseUri = uri.GetLeftPart(UriPartial.Authority);

            return baseUri;
        }
        catch (Exception ex)
        {
            _ = ex;
            return DEFAULT_ENDPOINT;
        }
    }
}
