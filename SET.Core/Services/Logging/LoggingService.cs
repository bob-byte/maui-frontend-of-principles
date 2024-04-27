using Serilog;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SET.Core.Services;

public class LoggingService : ILoggingService
{
    private readonly ISettingsService m_settingsService;

    public LoggingService( ISettingsService settingsService )
    {
        m_settingsService = settingsService;
    }

    public virtual void LogCriticalError( Exception exception )
    {
        Log.Error( exception, exception.Message );
        if (exception.InnerException != null)
        {
            Log.Error( exception.InnerException, exception.InnerException.Message );
        }

        if (m_settingsService.IsDebug)
        {
            Console.WriteLine( exception.ToString() );
        }
    }

    public virtual void LogCriticalError( string message, Exception exception )
    {
        Log.Error( exception, exception.Message );
        Log.Error( message );

        if (m_settingsService.IsDebug)
        {
            Console.WriteLine( exception.ToString() );
            Console.WriteLine( message.WithAttention() );
        }

        if (exception.InnerException != null)
        {
            Log.Error( exception.InnerException, exception.InnerException.Message );

            if (m_settingsService.IsDebug)
            {
                Console.WriteLine( exception.InnerException.ToString() );
            }
        }
    }

    public virtual void LogFatal( string message )
    {
        Log.Fatal( message );

        if (m_settingsService.IsDebug)
        {
            Console.WriteLine( message.WithAttention() );
        }
    }

    public virtual void LogError( Exception ex, string message )
    {
        Log.Error( ex, message );

        if (m_settingsService.IsDebug)
        {
            Console.WriteLine( message );
        }
    }

    public virtual void LogError( string message )
    {
        Log.Error( message );

        if (m_settingsService.IsDebug)
        {
            Console.WriteLine( message );
        }
    }

    public virtual void LogInfo( string message )
    {
        Log.Logger.Information( message );

        if (m_settingsService.IsDebug)
        {
            Console.WriteLine( message );
        }
    }

    public virtual void LogInfoWithLongTime( string message )
    {
        message = message + " UTC:" + DateTime.UtcNow.ToLongTimeString();
        LogInfo( message );
    }
}
