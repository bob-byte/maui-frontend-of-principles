
namespace Principles.Core.Services;

public class LocalRemoteExecutor : ILocalRemoteExecutor
{
    private readonly ISyncQueueService m_syncQueue;
    private readonly INetworkService m_networkService;

    public LocalRemoteExecutor( IServiceProvider serviceProvider )
    {
        m_syncQueue = serviceProvider.GetRequiredService<ISyncQueueService>();
        m_networkService = serviceProvider.GetRequiredService<INetworkService>();
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
            remoteCall().ContinueWith(async t =>
            {
                if (t.IsFaulted)
                {
                    await m_syncQueue.AddToQueueAsync(handlerType, operation, data).ConfigureAwait(false);
                }
            }).GetAwaiter();
        }
        else
        {
            await m_syncQueue.AddToQueueAsync(handlerType, operation, data).ConfigureAwait(false);
        }
    }
}
