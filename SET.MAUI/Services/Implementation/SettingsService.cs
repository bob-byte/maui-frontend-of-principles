
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
}
