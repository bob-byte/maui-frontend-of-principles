namespace Principles.ViewModels;

public partial class UpdatePopupViewModel : BaseViewModel
{
    private const string KEY_TO_STORE_DONT_SHOW_AGAIN_FOR_SOME_VERSION = "DontShowUpdatePopupAgainForVersion";

    [ObservableProperty]
    private string? m_versionDescription;

    [ObservableProperty]
    private bool m_dontShowAgain;

    private readonly IVersionCheckerService m_versionCheckerService;
    private AppVersionInfo? m_appVersionInfo;

    public UpdatePopupViewModel( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
        m_versionCheckerService = serviceProvider.GetRequiredService<IVersionCheckerService>();
    }

    public async Task<bool> ShouldShowPopup()
    {
        bool? result = null;

        try
        {
            string language = LocManager.CurrentCulture.Name;

            m_appVersionInfo = await m_versionCheckerService.GetAppVersionAsync( language );
            VersionDescription = m_appVersionInfo.VersionDescription;
        }
        catch
        {
            result = false;
        }

        if (result is null)
        {
            Version currentVersion = AppInfo.Version;
            Version newAvailableAppVersion = new( m_appVersionInfo!.AppVersion );

            bool isAvailableNewerVersion = newAvailableAppVersion > currentVersion;

            if (isAvailableNewerVersion)
            {
                string strVersionForWhichDontShowPopup = Preferences.Get( KEY_TO_STORE_DONT_SHOW_AGAIN_FOR_SOME_VERSION, defaultValue: string.Empty );
                if (string.IsNullOrWhiteSpace( strVersionForWhichDontShowPopup ))
                {
                    result = true;
                }
                else
                {
                    Version versionForWhichDontShowPopup = new( strVersionForWhichDontShowPopup );
                    result = versionForWhichDontShowPopup < newAvailableAppVersion;
                }
            }
            else
            {
                result = false;
            }
        }

        return result.Value;
    }

    [RelayCommand]
    private void OpenStore()
    {
        if (DontShowAgain)
        {
            Preferences.Set( KEY_TO_STORE_DONT_SHOW_AGAIN_FOR_SOME_VERSION, m_appVersionInfo!.AppVersion );
        }

        string url;
#if IOS
        string appIdInAppStore = "6503646940";
        string appStoreUrl = $"https://apps.apple.com/app/id{appIdInAppStore}";
        
        url = appStoreUrl;
#else
        string packageName = "com.set.principles";
        string googlePlayUrl = $"https://play.google.com/store/apps/details?id={packageName}";

        url = googlePlayUrl;
#endif

        BrowserHelper.OpenUrl( url ).GetAwaiter();
    }

    [RelayCommand]
    private void CancelUpdate()
    {
        if (DontShowAgain)
        {
            Preferences.Set( KEY_TO_STORE_DONT_SHOW_AGAIN_FOR_SOME_VERSION, m_appVersionInfo!.AppVersion );
        }
    }
}