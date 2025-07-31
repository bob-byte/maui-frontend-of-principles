namespace Principles.Core.Services;

public class OfflineApiService<T>
{
    private readonly IOfflineRepository<T> _localRepository;
    private readonly IRemoteApiService<T> _remoteApi;
    private readonly ISyncQueue<T> _syncQueue;
    private readonly IConnectivityService _connectivity;

    public OfflineApiService(
        IOfflineRepository<T> localRepository,
        IRemoteApiService<T> remoteApi,
        ISyncQueue<T> syncQueue,
        IConnectivityService connectivity)
    {
        _localRepository = localRepository;
        _remoteApi = remoteApi;
        _syncQueue = syncQueue;
        _connectivity = connectivity;
    }

    public async Task<IEnumerable<T>> GetAllAsync(bool forceRefresh = false)
    {
        if (_connectivity.IsConnected && forceRefresh)
        {
            var remoteItems = await _remoteApi.GetAllAsync();
            await _localRepository.SaveRangeAsync(remoteItems);
            return remoteItems;
        }

        return await _localRepository.GetAllAsync();
    }

    public async Task AddAsync(T item)
    {
        await _localRepository.AddAsync(item);

        if (_connectivity.IsConnected)
        {
            try
            {
                await _remoteApi.AddAsync(item);
            }
            catch
            {
                await _syncQueue.EnqueueAddAsync(item);
            }
        }
        else
        {
            await _syncQueue.EnqueueAddAsync(item);
        }
    }

    public async Task UpdateAsync(T item)
    {
        await _localRepository.UpdateAsync(item);

        if (_connectivity.IsConnected)
        {
            try
            {
                await _remoteApi.UpdateAsync(item);
            }
            catch
            {
                await _syncQueue.EnqueueUpdateAsync(item);
            }
        }
        else
        {
            await _syncQueue.EnqueueUpdateAsync(item);
        }
    }

    public async Task DeleteAsync(T item)
    {
        await _localRepository.DeleteAsync(item);

        if (_connectivity.IsConnected)
        {
            try
            {
                await _remoteApi.DeleteAsync(item);
            }
            catch
            {
                await _syncQueue.EnqueueDeleteAsync(item);
            }
        }
        else
        {
            await _syncQueue.EnqueueDeleteAsync(item);
        }
    }

    public async Task SyncAsync()
    {
        if (!_connectivity.IsConnected)
            return;

        await _syncQueue.ProcessAsync(_remoteApi);
    }
}
