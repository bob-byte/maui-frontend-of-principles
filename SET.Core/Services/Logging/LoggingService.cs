using Serilog;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SET.Core.Services;

public class LoggingService : ILoggingService
{
    public virtual void LogCriticalError( Exception exception )
    {
        Log.Error( exception, exception.Message );
        if (exception.InnerException != null)
        {
            Log.Error( exception.InnerException, exception.InnerException.Message );
        }
    }

    public virtual void LogCriticalError( string message, Exception exception )
    {
        Log.Error( exception, exception.Message );
        Log.Error( message );

        if (exception.InnerException != null)
        {
            Log.Error( exception.InnerException, exception.InnerException.Message );
        }
    }

    public virtual void LogFatal( string message )
    {
        Log.Fatal( message );
    }

    public virtual void LogError( Exception ex, string message )
    {
        Log.Error( ex, message );
    }

    public virtual void LogError( string message )
    {
        Log.Error( message );
    }

    public virtual void LogInfo( string message )
    {
        Log.Information( message );
    }

    public virtual void LogInfoWithLongTime( string message )
    {
        message = message + " UTC:" + DateTime.UtcNow.ToLongTimeString();
        LogInfo( message );
    }
}
