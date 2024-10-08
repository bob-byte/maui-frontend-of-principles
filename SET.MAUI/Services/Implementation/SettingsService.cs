
namespace SET.MAUI.Services;

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

    public string? AuthAccessToken { get; private set; }

    public async Task<string> GetAuthAccessTokenAsync()
    {
        string? token = await SecureStorage.GetAsync( key: "access_token" ).DefaultConfigureAwait();

        AuthAccessToken = token ?? string.Empty;
        return AuthAccessToken;
    }

    public async Task SetAuthAccessTokenAsync( string value )
    {
        await SecureStorage.SetAsync( "access_token", value );
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
