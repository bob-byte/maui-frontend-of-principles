
namespace Principles.Services;

public class SettingsService : ISettingsService
{
    public bool IsDebug
    {
        get
        {
            bool result;
#if DEBUG
            result = true;
#else
            result = false;
#endif
            return result;
        }
    }
    private const string ACCESS_TOKEN_KEY =
#if LOCAL_DEBUG
    "local_access_token";
#else
    "access_token";
#endif

    public string? AuthAccessToken { get; private set; }

    public async Task<string> GetAuthAccessTokenAsync()
    {
        string? token = await SecureStorage.GetAsync( key: ACCESS_TOKEN_KEY ).DefaultConfigureAwait();

        AuthAccessToken = token ?? string.Empty;
        return AuthAccessToken;
    }

    public async Task SetAuthAccessTokenAsync( string value )
    {
        await SecureStorage.SetAsync( ACCESS_TOKEN_KEY, value );
        AuthAccessToken = value;
    }

    public double NormalPageWidth
    {
        get => Preferences.Get( key: "normal_page_width", defaultValue: 0.0 );
        set => Preferences.Set( key: "normal_page_width", value );
    }

    public double NormalPageHeight
    {
        get => Preferences.Get( key: "normal_page_height", defaultValue: 0.0 );
        set => Preferences.Set( key: "normal_page_height", value );
    }
}
