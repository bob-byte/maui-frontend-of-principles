using Serilog.Core;
using Serilog.Events;

using System;
namespace SET.Core.Services;

public class LoggerToServer : ILogEventSink
{
    private readonly Func<LogEvent, IServiceProvider, SaveLogRequest> m_funcToCreateRequest;

    public LoggerToServer( Func<LogEvent, IServiceProvider, SaveLogRequest> funcToCreateRequest )
    {
        m_funcToCreateRequest = funcToCreateRequest;
    }

    public async void Emit( LogEvent logEvent )
    {
        IServiceLocator serviceLocator = ServiceLocator.Current ??
            throw new InvalidOperationException( $"{typeof( IServiceLocator )} is not inited after app start" );

        IUrlBuilder urlBuilder = serviceLocator.GetRequiredService<IUrlBuilder>();
        ISettingsService settingsService = serviceLocator.GetRequiredService<ISettingsService>();
        IRequestProvider requestProvider = serviceLocator.GetRequiredService<IRequestProvider>();

        SaveLogRequest request = m_funcToCreateRequest( logEvent, serviceLocator.ServiceProvider );
        string url = $"{urlBuilder.Logs}";

        try
        {
            await requestProvider.PostAsync( url, request, settingsService.AuthAccessToken );
        }
        catch(Exception ex)
        {
            Console.WriteLine( $"Cannot post client log on the server: {ex}" );
        }
    }
}

