namespace Principles.ViewModels;

public partial class UpdatePopupViewModel : BaseViewModel
{
    private const string KEY_TO_STORE_DONT_SHOW_AGAIN_FOR_SOME_VERSION = "DontShowUpdatePopupAgainForVersion";

    [ObservableProperty]
    private bool m_dontShowAgain;

    [ObservableProperty]
    private string m_versionDescription;

    private IVersionCheckerService m_versionCheckerService;

    public UpdatePopupViewModel( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
        m_versionCheckerService = serviceProvider.GetRequiredService<IVersionCheckerService>();
        VersionDescription = m_versionCheckerService.VersionDescription;
    }

    [RelayCommand]
    private async Task OpenStoreAsync()
    {
        if (DontShowAgain)
        {
            Preferences.Set( KEY_TO_STORE_DONT_SHOW_AGAIN_FOR_SOME_VERSION, m_versionCheckerService.LatestAppVersion.ToString() );
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

        await BrowserHelper.OpenUrl( url );

        await DialogService.ClosePopupAsync();
    }

    [RelayCommand]
    private async Task CancelUpdateAsync()
    {
        if (DontShowAgain)
        {
            Preferences.Set( KEY_TO_STORE_DONT_SHOW_AGAIN_FOR_SOME_VERSION, m_versionCheckerService.LatestAppVersion.ToString() );
        }

        await DialogService.ClosePopupAsync();
    }
}