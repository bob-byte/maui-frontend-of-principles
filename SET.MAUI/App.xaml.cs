using Microsoft.Maui;
using Microsoft.Maui.Controls;

using Application = Microsoft.Maui.Controls.Application;

namespace SET.MAUI;

public partial class App : Application
{
    public App( INavigationService navigationService, IServiceLocator serviceLocator )
    {
        ServiceLocator.GetCurrentLocator = () => serviceLocator;

        //uncaught exception for iOS is handled in the Program.cs file
#if ANDROID
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
#endif

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
            string errorMsg = $"Sender of uncaught exception is: {sender?.ToString()}. Error msg:" +
                e?.ExceptionObject?.ToString() ?? string.Empty;

            loggingService.LogFatal( errorMsg );
        }

        //wait while Serilog send client log to the server
        Thread.Sleep( millisecondsTimeout: 100 );
    }
}
