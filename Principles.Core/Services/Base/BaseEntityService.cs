namespace Principles.Core.Services;

public class BaseEntityService<T> : BaseRemoteService where T : IEntity, new()
{
    protected OfflineApiService<T> OfflineApiService { get; }
    protected IRemoteApiService<T> RemoteApiService { get; }
    protected IOfflineRepository OfflineRepository { get; }
    protected ISyncQueueService SyncQueueService { get; }
    
    public BaseEntityService(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
        OfflineRepository = serviceProvider.GetRequiredService<IOfflineRepository>();
        RemoteApiService = serviceProvider.GetRequiredService<IRemoteApiService<T>>();
        SyncQueueService = serviceProvider.GetRequiredService<ISyncQueueService>();
        OfflineApiService = new OfflineApiService<T>( serviceProvider );
    }
}