using OpenAI.Chat;

using Principles.Core.Constants;

using Principles.Core.Services.AiKey;

namespace Principles.Core.Services;

public abstract class RemoteApiService<T> : BaseRemoteService, ISyncQueueHandler
{
    protected IDatabase Database { get; }
    protected ISyncQueueService SyncQueue { get; }
    protected INetworkService NetworkService { get; }

    public RemoteApiService( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
        Database = serviceProvider.GetRequiredService<IDatabase>();
        SyncQueue = serviceProvider.GetRequiredService<ISyncQueueService>();
        NetworkService = serviceProvider.GetRequiredService<INetworkService>();
    }

    public virtual bool CanHandle( string entityType )
    {
        return entityType == typeof( T ).Name;
    }

    public abstract Task HandleQueueItemAsync( SyncQueueItem queueItem );
}
