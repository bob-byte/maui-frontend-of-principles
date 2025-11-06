
namespace Principles.Core.Services;

public class OfflineApiService<T> : IOfflineApiService<T> where T: IEntity, new()
{
    private readonly IOfflineRepository m_localRepository;
    private readonly RemoteApiService<T> m_remoteApi;
    private readonly ISyncQueueService m_syncQueue;
    private readonly INetworkService m_networkService;

    public OfflineApiService(IServiceProvider serviceProvider)
    {
        m_localRepository = serviceProvider.GetRequiredService<IOfflineRepository>();
        m_remoteApi = serviceProvider.GetRequiredService<RemoteApiService<T>>();
        m_syncQueue = serviceProvider.GetRequiredService<ISyncQueueService>();
        m_networkService = serviceProvider.GetRequiredService<INetworkService>();
    }

    public async Task<List<T>> GetAllAsync(bool forceRefresh = false)
    {
        if (m_networkService.IsConnected && forceRefresh)
        {
            List<T> remoteItems = await m_remoteApi.GetAllAsync();
            await m_localRepository.SaveRangeAsync(remoteItems);
            return remoteItems;
        }

        return await m_localRepository.GetAllAsync<T>();
    }

    public async Task SaveAsync(T item)
    {
        await m_localRepository.SaveAsync(item);

        if (m_networkService.IsConnected)
        {
            try
            {
                await m_remoteApi.SaveAsync(item);
            }
            catch
            {
                await m_syncQueue.AddToQueueAsync(item, OperationType.Save);
            }
        }
        else
        {
            await m_syncQueue.AddToQueueAsync(item, OperationType.Save);
        }
    }

    public async Task ExecuteAsync(Func<Task> localChange, string handlerType, string operation, object? data)
    {
        await localChange();

        if (m_networkService.IsConnected)
        {
            try
            {
                await m_remoteApi.ExecuteAsync(operation, data);
            }
            catch
            {
                await m_syncQueue.AddToQueueAsync(handlerType, operation, data);
            }
        }
        else
        {
            await m_syncQueue.AddToQueueAsync(handlerType, operation, data);
        }
    }
    
    public async Task DeleteAsync(T item)
    {
        await m_localRepository.DeleteAsync(item);

        if (m_networkService.IsConnected)
        {
            try
            {
                await m_remoteApi.DeleteAsync(item);
            }
            catch
            {
                await m_syncQueue.AddToQueueAsync(item, OperationType.Delete);
            }
        }
        else
        {
            await m_syncQueue.AddToQueueAsync(item, OperationType.Delete);
        }
    }
}
