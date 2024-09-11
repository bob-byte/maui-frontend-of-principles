using CommunityToolkit.Maui.Views;

using Application = Microsoft.Maui.Controls.Application;

namespace SET.MAUI;

public partial class App : Application
{
    private readonly UpdatePopupViewModel m_updatePopupViewModel;
    public App( IServiceLocator serviceLocator, IServiceProvider serviceProvider)
    {
        ServiceLocator.GetCurrentLocator = () => serviceLocator;

        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

        m_updatePopupViewModel = new UpdatePopupViewModel( serviceProvider );

        UserAppTheme = AppTheme.Light;
        InitializeComponent();
        MainPage = new AppShell( serviceProvider );
    }
    protected async override void OnStart( )
    {
        bool shouldShowPopup = await m_updatePopupViewModel.ShouldShowPopup();

        if (shouldShowPopup)
        {
            var popup = new UpdatePopup( m_updatePopupViewModel );
            Current.MainPage.ShowPopup( popup );
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
