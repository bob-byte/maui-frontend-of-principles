namespace Principles.ViewModels;

public partial class UpdatePopupViewModel : BaseViewModel
{
    private const string KEY_TO_STORE_DONT_SHOW_AGAIN_FOR_SOME_VERSION = "DontShowUpdatePopupAgainForVersion";

    [ObservableProperty]
    private string? m_versionDescription;

    [ObservableProperty]
    private bool m_dontShowAgain;

    private readonly IAppStoreInfo m_appStoreInfo;
    private AppStoreInformation? m_appStoreInformation;
    

    public UpdatePopupViewModel( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
        m_appStoreInfo =  DependencyService.Get<IAppStoreInfo>();
    }

    public async Task<bool> ShouldShowPopup()
    {
    try
    {
        m_appStoreInformation = await m_appStoreInfo.GetInformationAsync();
        
        VersionDescription = m_appStoreInformation.ReleaseNotes;

        Version currentVersion = AppInfo.Version;
        Version storeVersion = m_appStoreInformation.LatestVersion;

        if (storeVersion > currentVersion)
        {
            string strVersionForWhichDontShowPopup = Preferences.Get(KEY_TO_STORE_DONT_SHOW_AGAIN_FOR_SOME_VERSION, string.Empty);

            if (string.IsNullOrWhiteSpace(strVersionForWhichDontShowPopup))
            {
                return true;
            }

            Version ignoredVersion = new(strVersionForWhichDontShowPopup);
            return ignoredVersion < storeVersion;
        }
    }
    catch (Exception ex)
    {
        return false;
    }

    return false;
    }

    [RelayCommand]
    private async Task OpenStore()
    {
        if (DontShowAgain && m_appStoreInformation != null)
        {
            Preferences.Set(KEY_TO_STORE_DONT_SHOW_AGAIN_FOR_SOME_VERSION, m_appStoreInformation.LatestVersion.ToString());
        }

        await m_appStoreInfo.OpenApplicationInStoreAsync();
    }

    [RelayCommand]
    private void CancelUpdate()
    {
        if (DontShowAgain && m_appStoreInformation != null)
        {
            Preferences.Set(KEY_TO_STORE_DONT_SHOW_AGAIN_FOR_SOME_VERSION, m_appStoreInformation.LatestVersion.ToString());
        }
    }
}