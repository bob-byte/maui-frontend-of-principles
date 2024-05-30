using ObjCRuntime;

using Serilog;

using UIKit;

namespace SET.MAUI;

public class Program
{
    // This is the main entry point of the application.
    static void Main(string[] args)
    {
        try
        {
            // if you want to use a different Application Delegate class from "AppDelegate"
            // you can specify it here.
            UIApplication.Main( args, null, typeof( AppDelegate ) );
        }
        catch (Exception ex)
        {
            IServiceLocator? serviceLocator = ServiceLocator.Current;
            ILoggingService? loggingService = serviceLocator?.GetServiceOrNull<ILoggingService>();

            if (serviceLocator is null || loggingService is null)
            {
                Log.Fatal( ex, ex.Message );
                if(ex.InnerException != null)
                {
                    Log.Fatal( ex.InnerException, $"Inner exception: {ex.InnerException.Message}" );
                }
            }
            else
            {
                loggingService.LogFatal( ex, message: "Uncaught fatal error" );
            }

            // wait for Serilog to send new logs to the server
            Thread.Sleep( millisecondsTimeout: 1000 );

            throw;
        }
    }
}