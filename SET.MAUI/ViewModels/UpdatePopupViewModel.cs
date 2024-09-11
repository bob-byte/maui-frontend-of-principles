namespace SET.MAUI.ViewModels;

public partial class UpdatePopupViewModel : BaseViewModel
{
    [ObservableProperty]
    private string m_versionDescription;
    [ObservableProperty]
    private double m_pageWidth;

    [ObservableProperty]
    private bool m_dontShowAgain;

    private readonly IVersionCheckerService m_versionChekerService;
    private AppVersionInfo m_appVersionInfo;
    public UpdatePopupViewModel( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
        m_versionChekerService = serviceProvider.GetRequiredService<IVersionCheckerService>();
    }

    public async Task<bool> ShouldShowPopup()
    {
        string language = CultureInfo.CurrentUICulture.Name;
        string currentVersion = VersionTracking.CurrentVersion;
        m_appVersionInfo = await m_versionChekerService.GetAppVersionAsync( language );
        string currentClientVersion = Preferences.Get( "DontShowPopupAgainForVersion", currentVersion );
        VersionDescription = m_appVersionInfo.VersionDescription;

        if (currentClientVersion != m_appVersionInfo.AppVersion)
        {
            return true;
        }

        return currentVersion != m_appVersionInfo.AppVersion;
    }

    [RelayCommand]
    private async Task OpenAppStoreAsync()
    {
        if (DontShowAgain)
        {
            Preferences.Set( "DontShowPopupAgainForVersion", m_appVersionInfo.AppVersion );
        }
        await Launcher.OpenAsync( new Uri( "https://testflight.apple.com/join/3oXW7gxy" ) );
    }

    [RelayCommand]
    private void CancelUpdate()
    {
        if (DontShowAgain)
        {
            Preferences.Set( "DontShowPopupAgainForVersion", m_appVersionInfo.AppVersion );
        }
    }
}