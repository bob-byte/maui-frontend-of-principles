namespace Principles.Core.Services;

public interface ILoggingService
{
    void LogError( string logRecord );

    void LogError( Exception ex, string logRecord );

    void LogCriticalError( Exception exception );

    void LogCriticalError( Exception exception, string message );

    void LogFatal( string message );

    void LogFatal( Exception exception );

    void LogFatal( Exception exception, string message );

    void LogInfo( string logRecord );

    void LogInfoWithLongTime( string logRecord );
}