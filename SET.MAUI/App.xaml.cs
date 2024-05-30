using Microsoft.Maui;
using Microsoft.Maui.Controls;

using Application = Microsoft.Maui.Controls.Application;

namespace SET.MAUI;

public partial class App : Application
{
    public App( INavigationService navigationService, IServiceLocator serviceLocator )
    {
        ServiceLocator.GetCurrentLocator = () => serviceLocator;

        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

        UserAppTheme = AppTheme.Light;
        InitializeComponent();

        MainPage = new AppShell( navigationService );
    }

    private void CurrentDomain_UnhandledException( object sender, UnhandledExceptionEventArgs e )
    {
        IServiceLocator serviceLocator = ServiceLocator.Current!;
        ILoggingService loggingService = serviceLocator.GetService<ILoggingService>();

        if (e?.ExceptionObject is Exception ex)
        {
            loggingService.LogFatal( ex );
        }
        else
        {
            string errorMsg = e?.ExceptionObject?.ToString() ?? string.Empty;
            string logRecord = $"Sender of uncaught exception is: {sender?.ToString()}. Error msg:" +
                errorMsg;

            loggingService.LogFatal( logRecord );
        }

        // wait for Serilog to send new logs to the server
        Thread.Sleep( millisecondsTimeout: 2000 );
    }
}
