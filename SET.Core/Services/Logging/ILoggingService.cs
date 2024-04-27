namespace SET.Core.Services;

public interface ILoggingService
{
    void LogCriticalError( Exception exception );

    void LogCriticalError( string message, Exception exception );

    void LogFatal( string message );

    void LogInfo( string logRecord );

    void LogError( string logRecord );

    void LogError( Exception ex, string logRecord );

    void LogInfoWithLongTime( string logRecord );
}