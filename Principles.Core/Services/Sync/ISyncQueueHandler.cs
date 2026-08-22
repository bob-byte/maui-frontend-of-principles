namespace Principles.Core.Services;

public interface ISyncQueueHandler
{
    bool CanHandle(string handlerType);
    Task HandleQueueItemAsync(SyncQueueItem queueItem);
}
