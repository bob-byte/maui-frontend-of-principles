
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
        string token = await SecureStorage.GetAsync( "access_token" ).DefaultConfigureAwait();
        AuthAccessToken = token ?? string.Empty;
        return AuthAccessToken;
    }

    public async Task SetAuthAccessTokenAsync( string value )
    {
        await SecureStorage.SetAsync( "access_token", value );
        AuthAccessToken = value;
    }

    public string UserId
    {
        get => Preferences.Get( key: "user_id", defaultValue: "0" )!;
        set => Preferences.Set( key: "user_id", value );
    }
}
