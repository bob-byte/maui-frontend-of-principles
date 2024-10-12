
namespace Principles.Core.Extensions;

public static class ValueTaskExtensions
{
    public static ConfiguredValueTaskAwaitable DefaultConfigureAwait( this ValueTask task )
    {
        return task.ConfigureAwait( continueOnCapturedContext: false );
    }

    public static ConfiguredValueTaskAwaitable<TResult> DefaultConfigureAwait<TResult>( this ValueTask<TResult> task )
    {
        return task.ConfigureAwait( continueOnCapturedContext: false );
    }
}
