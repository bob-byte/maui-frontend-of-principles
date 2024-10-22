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
            string language = CultureInfo.CurrentUICulture.Name;

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
        url = "https://testflight.apple.com/join/3oXW7gxy";
#else
        url = "https://play.google.com/apps/internaltest/4701722005129923451";
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