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

        IUrlBuilder urlBuilder = serviceLocator.GetService<IUrlBuilder>();
        ISettingsService settingsService = serviceLocator.GetService<ISettingsService>();
        IRequestProvider requestProvider = serviceLocator.GetService<IRequestProvider>();

        SaveLogRequest request = m_funcToCreateRequest( logEvent, serviceLocator.ServiceProvider );
        string url = $"{urlBuilder.Log}";
        await requestProvider.PutAsync( url, request, settingsService.AuthAccessToken );
    }
}

