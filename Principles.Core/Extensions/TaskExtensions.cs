
namespace Principles.Core.Extensions;

public static class TaskExtensions
{
    public static ConfiguredTaskAwaitable DefaultConfigureAwait( this Task task )
    {
        return task.ConfigureAwait( continueOnCapturedContext: false );
    }

    public static ConfiguredTaskAwaitable<TResult> DefaultConfigureAwait<TResult>( this Task<TResult> task )
    {
        return task.ConfigureAwait( continueOnCapturedContext: false );
    }
}
