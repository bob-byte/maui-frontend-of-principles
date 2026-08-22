namespace Principles.Core.Services;

public interface ILocalRemoteExecutor
{
    Task ExecuteAsync<TEntity>( Func<Task> localCall, Func<Task> remoteCall, OperationKind operation, object? data )
        where TEntity : class, IEntity;
}
