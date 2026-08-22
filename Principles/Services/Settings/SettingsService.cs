
namespace Principles.Services;

public class SettingsService : ISettingsService
{
    private const string ACCESS_TOKEN_KEY =
#if LOCALDEBUG
        "local_access_token";
#else
        "access_token";
#endif
    private const string LAST_SUCCESSFUL_SYNC_AT_KEY = "last_successful_sync_at";
    private const string LAST_FAILED_SYNC_AT_KEY = "last_failed_sync_at";

    public bool IsDebug
    {
        get
        {
            bool result;
#if DEBUG || LOCALDEBUG
            result = true;
#else
            result = false;
#endif
            return result;
        }
    }

    public string? AuthAccessToken { get; private set; }

    public DateTime? LastSuccessfulSyncAt
    {
        get => GetStoredDateTime( LAST_SUCCESSFUL_SYNC_AT_KEY );
        set => SetStoredDateTime( LAST_SUCCESSFUL_SYNC_AT_KEY, value );
    }

    public DateTime? LastFailedSyncAt
    {
        get => GetStoredDateTime( LAST_FAILED_SYNC_AT_KEY );
        set => SetStoredDateTime( LAST_FAILED_SYNC_AT_KEY, value );
    }

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
        LastSuccessfulSyncAt = null;
        LastFailedSyncAt = null;
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
    public string CurrentCulture { get; set; }

    private static DateTime? GetStoredDateTime( string key )
    {
        string? storedValue = Preferences.Get( key, null );
        if (string.IsNullOrWhiteSpace( storedValue ) ||
            !DateTime.TryParse( storedValue, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime parsedValue ))
        {
            return null;
        }

        return parsedValue;
    }

    private static void SetStoredDateTime( string key, DateTime? value )
    {
        if (value is null)
        {
            Preferences.Remove( key );
            return;
        }

        Preferences.Set( key, value.Value.ToString( "O", CultureInfo.InvariantCulture ) );
    }
}
