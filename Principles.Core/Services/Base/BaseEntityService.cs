namespace Principles.Core.Services;

public class BaseEntityService<T> : BaseRemoteService where T : IEntity, new()
{
    protected IDatabase Database { get; }
    protected ISyncQueueService SyncQueueService { get; }
    protected ILocalRemoteExecutor LocalRemoteExecutor { get; }

    public BaseEntityService(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
        Database = serviceProvider.GetRequiredService<IDatabase>();
        SyncQueueService = serviceProvider.GetRequiredService<ISyncQueueService>();
        LocalRemoteExecutor = serviceProvider.GetRequiredService<ILocalRemoteExecutor>();
    }
}
