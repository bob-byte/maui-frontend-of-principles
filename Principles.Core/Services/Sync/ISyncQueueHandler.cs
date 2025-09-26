namespace Principles.Core.Services;

public interface ISyncQueueHandler
{
    bool CanHandle(string handlerType);
    Task SaveAsync(long entityId, long localId, string? payloadJson);
    Task DeleteAsync(long entityId, string? payloadJson);
    Task ExecuteAsync(string operation, string? payloadJson);
}