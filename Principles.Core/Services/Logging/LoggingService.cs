using Serilog;

namespace Principles.Core.Services;

public class LoggingService : ILoggingService
{
    public virtual void LogCriticalError( Exception exception )
    {
        Log.Error( exception, exception.Message );

        if (exception.InnerException != null)
        {
            Log.Error( exception.InnerException, $"InnerException message: {exception.InnerException.Message}" );
        }
    }

    public virtual void LogCriticalError( Exception exception, string message )
    {
        Log.Error( exception, message );

        if (exception.InnerException != null)
        {
            Log.Error( exception.InnerException, $"InnerException message: {exception.InnerException.Message}" );
        }
    }

    public virtual void LogFatal( Exception exception, string message )
    {
        Log.Fatal( exception, message );

        if (exception.InnerException != null)
        {
            Log.Fatal( exception.InnerException, $"InnerException message: {exception.InnerException.Message}" );
        }
    }

    public virtual void LogFatal( Exception exception )
    {
        Log.Fatal( exception, exception.Message );

        if (exception.InnerException != null)
        {
            Log.Fatal( exception.InnerException, $"InnerException message: {exception.InnerException.Message}" );
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
