using CommunityToolkit.Maui.Views;

using Plugin.AdMob.Services;

using Principles.Core.Services.AiKey;

using Application = Microsoft.Maui.Controls.Application;

namespace Principles;

public partial class App : Application
{
    private readonly UpdatePopupViewModel m_updatePopupViewModel;
    private readonly ISettingsService m_settingsService;
    private readonly ILoggingService m_loggingService;
    private readonly IAdService m_adService;
    private readonly IAppOpenTrackerService m_appOpenTracker;

    private UpdatePopup? m_updatePopup;
    
    public App( IServiceProvider serviceProvider )
    {
        IServiceLocator serviceLocator = serviceProvider.GetRequiredService<IServiceLocator>();
        ServiceLocator.GetCurrentLocator = () => serviceLocator;

        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        
        m_settingsService = serviceProvider.GetRequiredService<ISettingsService>();
        m_loggingService = serviceProvider.GetRequiredService<ILoggingService>();
        m_adService = serviceProvider.GetRequiredService<IAdService>();
        m_appOpenTracker = serviceProvider.GetRequiredService<IAppOpenTrackerService>();

        m_updatePopupViewModel = new UpdatePopupViewModel( serviceProvider );

        UserAppTheme = AppTheme.Light;

        string? savedLanguageCode = Preferences.Get( "AppLanguage", null );
        
        if (string.IsNullOrWhiteSpace( savedLanguageCode )) 
        {
            savedLanguageCode = CultureInfo.CurrentUICulture.Name;
            Preferences.Set( "AppLanguage", savedLanguageCode );
        }

        CultureInfo currentCulture = new( savedLanguageCode );

        LocalizationResourceManager.Initialize( m_settingsService );
        LocalizationResourceManager.Instance.SetCulture( currentCulture );
        
        InitializeComponent();
    }

    protected override Window CreateWindow( IActivationState? activationState )
    {
        return new Window( new AppShell( ServiceLocator.Current!.ServiceProvider ) );
    }

    protected async override void OnStart()
    {
        base.OnStart();

        m_appOpenTracker.TrackAppOpen();

        if (m_settingsService.IsAdsEnabled)
        {
            await m_adService.GetAccessToTrackAsync();
        }
        
        LocalNotificationCenter.Current.ClearAll();

        if (VersionTracking.IsFirstLaunchEver || VersionTracking.IsFirstLaunchForCurrentBuild || VersionTracking.IsFirstLaunchForCurrentVersion) 
        {
            await SecureStorage.SetAsync( CacheKeys.API_KEY, string.Empty );
        }
        
        m_loggingService.LogInfo( "App starting..." );
        
        if (VersionTracking.IsFirstLaunchEver)
        {
            try
            {
                string token = await m_settingsService.GetAuthAccessTokenAsync();
                if (!string.IsNullOrWhiteSpace( token ))
                {
                    IReminderService reminderService = ServiceLocator.Current!.GetRequiredService<IReminderService>();
                    await reminderService.TryToRecoverAllUserRemindersAsync();
                }
            }
            catch (Exception ex)
            {
                m_loggingService.LogError( ex, ex.Message );
            }
        }
        
        bool shouldShowPopup = await m_updatePopupViewModel.ShouldShowPopup();
        
        if (shouldShowPopup)
        {
            m_updatePopup ??= new UpdatePopup( m_updatePopupViewModel );
            await Shell.Current.ShowPopupAsync( m_updatePopup );
        }
    }

    protected override async void OnResume()
    {
        base.OnResume();
        
        m_appOpenTracker.TrackAppOpen();
        
        m_adService.LoadInterstitialAd();
        
        m_loggingService.LogInfo( "App resuming..." );
        
        LocalNotificationCenter.Current.ClearAll();
        
        if (VersionTracking.IsFirstLaunchEver || VersionTracking.IsFirstLaunchForCurrentBuild || VersionTracking.IsFirstLaunchForCurrentVersion) 
        {
            await SecureStorage.SetAsync( CacheKeys.API_KEY, string.Empty );
        }
        
        m_updatePopupViewModel.ReferenceMessenger.Send( new TryAddNewDayInHabitListMessage() );

        bool shouldShowPopup = (m_updatePopup is null || !m_updatePopup.IsShown) && ( await m_updatePopupViewModel.ShouldShowPopup());
        
        if (shouldShowPopup)
        {
            m_updatePopup = new UpdatePopup( m_updatePopupViewModel );
            await Shell.Current!.ShowPopupAsync( m_updatePopup );
        }
    }

    protected override void OnSleep()
    {
        m_adService.CleanupAds();
        
        base.OnSleep();
    }

    private void CurrentDomain_UnhandledException( object sender, UnhandledExceptionEventArgs e )
    {
        IServiceLocator serviceLocator = ServiceLocator.Current!;
        ILoggingService loggingService = serviceLocator.GetRequiredService<ILoggingService>();

        if (e?.ExceptionObject is Exception ex)
        {
            loggingService.LogFatal( ex );
        }
        else
        {
            string errorMsg = e?.ExceptionObject?.ToString() ?? string.Empty;
            string logRecord = $"Sender of uncaught exception is: {sender?.ToString()}. Error msg: {errorMsg}";

            loggingService.LogFatal( logRecord );
        }

        // wait for Serilog to send new logs to the server
        Thread.Sleep( millisecondsTimeout: 2000 );
    }
}
