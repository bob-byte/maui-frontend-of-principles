
namespace Principles.Core.Services;
public class VersionCheckerService : BaseRemoteService, IVersionCheckerService
{
    public VersionCheckerService( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
        //do nothing
    }
    public async Task<AppVersionInfo> GetAppVersionAsync(string language)
    {
        string url = $"{UrlBuilder.VersionCheck}?language={language}&osPlatform={DeviceInfo.Platform}";
        AppVersionInfo appVersion = await RequestProvider.GetAsync<AppVersionInfo>(
            url
        ).DefaultConfigureAwait();
        return appVersion;
    }

    public string VersionDescription { get; private set; } = string.Empty;
    public Version LatestAppVersion { get; private set; }

    public async Task<bool> ShouldShowPopup()
    {
        if (SettingsService.IsDebug)
        {
            return false;
        }
        
        bool? result = null;

        try
        {
            string language = SettingsService.CurrentCulture;

            AppVersionInfo appVersionInfo = await GetAppVersionAsync( language ).DefaultConfigureAwait();
            LatestAppVersion = new( appVersionInfo.AppVersion );
            VersionDescription = appVersionInfo.VersionDescription;
        }
        catch
        {
            result = false;
        }

        if (result is null)
        {
            Version currentVersion = AppInfo.Version;

            bool isAvailableNewerVersion = LatestAppVersion > currentVersion;

            if (isAvailableNewerVersion)
            {
                const string STORAGE_KEY = "DontShowUpdatePopupAgainForVersion";
                string strVersionForWhichDontShowPopup = Preferences.Get( STORAGE_KEY, defaultValue: string.Empty );

                if (string.IsNullOrWhiteSpace( strVersionForWhichDontShowPopup ))
                {
                    result = true;
                }
                else
                {
                    Version versionForWhichDontShowPopup = new( strVersionForWhichDontShowPopup );
                    result = versionForWhichDontShowPopup < LatestAppVersion;
                }
            }
            else
            {
                result = false;
            }
        }

        return result.Value;
    }
}
