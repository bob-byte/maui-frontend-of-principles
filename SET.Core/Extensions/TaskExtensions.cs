using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SET.Core.Extensions;

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
