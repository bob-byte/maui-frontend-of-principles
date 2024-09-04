using Microsoft.Maui;
using Microsoft.Maui.Controls;

using Application = Microsoft.Maui.Controls.Application;

namespace SET.MAUI;

public partial class App : Application
{
    private readonly IVersionCheckerService m_versionChekerService;
    private readonly IDialogService m_dialogService;
    public App( IServiceLocator serviceLocator, IServiceProvider serviceProvider)
    {
        ServiceLocator.GetCurrentLocator = () => serviceLocator;

        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

        m_versionChekerService = serviceProvider.GetRequiredService<IVersionCheckerService>();
        m_dialogService = serviceProvider.GetRequiredService<IDialogService>();

        UserAppTheme = AppTheme.Light;
        InitializeComponent();
        MainPage = new AppShell( serviceProvider );
    }
    protected override async void OnStart()
    {
        await VersionCompareAsync();
    }

    private async Task VersionCompareAsync()
    {
        string language = CultureInfo.CurrentUICulture.Name;
        string currentVersion = VersionTracking.CurrentVersion;
        AppVersionInfo appVersionInfo = await m_versionChekerService.GetAppVersionAsync( language );
        if (currentVersion != appVersionInfo.RelevantVersion)
        {
            bool shouldUpdate = await m_dialogService.ShowAlertWithTwoBtnsAsync(
                appVersionInfo.VersionDescription,
                LocStrings.AppUpdateAvailable,
                LocStrings.AppUpdateButton,
                LocStrings.AppUpdateCloseButton
            );

            if (shouldUpdate)
            {
                await Launcher.OpenAsync( new Uri( "https://testflight.apple.com/join/3oXW7gxy" ) );
            }
        }
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
