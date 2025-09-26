using OpenAI.Chat;

using Principles.Core.Services.AiKey;

namespace Principles.Core.Services;

public abstract class RemoteApiService<T> : BaseRemoteService, ISyncQueueHandler, IRemoteApiService<T>
{
    protected IOfflineRepository OfflineRepository { get; }
    protected ISyncQueueService SyncQueue { get; }
    protected INetworkService NetworkService { get; }

    public RemoteApiService(IServiceProvider serviceProvider)
        : base( serviceProvider )
    {
        OfflineRepository = serviceProvider.GetRequiredService<IOfflineRepository>();
        SyncQueue = serviceProvider.GetRequiredService<ISyncQueueService>();
        NetworkService = serviceProvider.GetRequiredService<INetworkService>();
    }
    
    public virtual bool CanHandle( string entityType )
    {
        return entityType == typeof(T).Name;
    }
    
    public abstract Task<List<T>> GetAllAsync(bool forceRefresh = false);

    public abstract Task SaveAsync( long entityId, long localId, string payloadJson );
    public abstract Task SaveAsync( T item );

    public abstract Task DeleteAsync( long entityId, string payloadJson );

    public abstract Task DeleteAsync( T item );
    public abstract Task ExecuteAsync( string operation, string? payloadJson );

    public virtual Task ExecuteAsync( string operation, object? data )
    {
        string? payloadJson = data is null ? null : JsonSerializer.Serialize( data );
        return ExecuteAsync( operation, payloadJson );
    }
}