using Principles.Models;

using System.Text.Json;

namespace Principles.ViewModels;

public partial class UpdatePopupViewModel : BaseViewModel
{
    private const string KEY_TO_STORE_DONT_SHOW_AGAIN_FOR_SOME_VERSION = "DontShowUpdatePopupAgainForVersion";

    [ObservableProperty]
    private bool m_dontShowAgain;

    private readonly IAppStoreInfo m_appStoreInfo;
    private AppStoreInformation? m_appStoreInformation;

    [ObservableProperty]
    private string? m_versionDescription;

    public UpdatePopupViewModel( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
        m_appStoreInfo = DependencyService.Get<IAppStoreInfo>();
        m_appStoreInformation = m_appStoreInfo.CachedInformation;
        if (m_appStoreInformation != null)
        {
            VersionDescription = m_appStoreInformation.ReleaseNotes;
        }
    }

    public static async Task<bool> ShouldShowUpdatePopup()
    {
        ISettingsService settingsService = ServiceLocator.Current!.GetRequiredService<ISettingsService>();

        if (settingsService.IsDebug)
        {
            return false;
        }
        
        bool? result = null;
        Version? latestVersion = null;

        try
        {
            string language = settingsService.CurrentCulture;

            IAppStoreInfo appStoreInfo = DependencyService.Get<IAppStoreInfo>();
            AppStoreInformation appStoreInformation = await appStoreInfo.GetInformationAsync();
            latestVersion = appStoreInformation.LatestVersion;
        }
        catch(Exception ex)
        {
            if(settingsService.IsDebug)
            {
                ILoggingService loggingService = ServiceLocator.Current!.GetRequiredService<ILoggingService>();
                loggingService.LogError(ex, logRecord: $"Failed to get app store information");
            }

            result = false;
        }

        if (result is null)
        {
            Version currentVersion = AppInfo.Version;

            bool isAvailableNewerVersion = latestVersion > currentVersion;

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
                    result = versionForWhichDontShowPopup < latestVersion;
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
    private async Task OpenStore()
    {
        if (DontShowAgain && m_appStoreInformation != null)
        {
            Preferences.Set( KEY_TO_STORE_DONT_SHOW_AGAIN_FOR_SOME_VERSION, m_appStoreInformation.LatestVersion.ToString() );
        }

        await m_appStoreInfo.OpenApplicationInStoreAsync();
    }

    [RelayCommand]
    private async Task CancelUpdateAsync()
    {
        if (DontShowAgain && m_appStoreInformation != null)
        {
            Preferences.Set( KEY_TO_STORE_DONT_SHOW_AGAIN_FOR_SOME_VERSION, m_appStoreInformation.LatestVersion.ToString() );
        }

        await DialogService.ClosePopupAsync();
    }
}