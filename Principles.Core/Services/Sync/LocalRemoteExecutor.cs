
namespace Principles.Core.Services;

public class LocalRemoteExecutor : ILocalRemoteExecutor
{
    private readonly ISyncQueueService m_syncQueue;
    private readonly INetworkService m_networkService;
    private readonly ILoggingService m_loggingService;

    public LocalRemoteExecutor( IServiceProvider serviceProvider )
    {
        m_syncQueue = serviceProvider.GetRequiredService<ISyncQueueService>();
        m_networkService = serviceProvider.GetRequiredService<INetworkService>();
        m_loggingService = serviceProvider.GetRequiredService<ILoggingService>();
    }

    public virtual Task ExecuteAsync<TEntity>(Func<Task> localCall, Func<Task> remoteCall, OperationKind operation, object? data)
        where TEntity : class, IEntity
    {
        return ExecuteAsync( localCall, remoteCall, typeof( TEntity ).Name, operation, data );
    }

    private async Task ExecuteAsync(Func<Task> localCall, Func<Task> remoteCall, string handlerType, OperationKind operation, object? data)
    {
        await localCall().ConfigureAwait(false);

        if (m_networkService.IsConnected)
        {
            _ = ExecuteRemoteCallAsync( handlerType, operation, data, remoteCall );
        }
        else
        {
            await m_syncQueue.AddToQueueAsync(handlerType, operation, data).ConfigureAwait(false);
        }
    }

    private async Task ExecuteRemoteCallAsync( string handlerType, OperationKind operation, object? data, Func<Task> remoteCall )
    {
        try
        {
            await remoteCall().ConfigureAwait( false );
        }
        catch (Exception ex)
        {
            m_loggingService.LogError( ex, $"Remote operation {handlerType}.{operation} failed. Falling back to sync queue." );
            await m_syncQueue.AddToQueueAsync( handlerType, operation, data ).ConfigureAwait( false );
        }
    }
}
