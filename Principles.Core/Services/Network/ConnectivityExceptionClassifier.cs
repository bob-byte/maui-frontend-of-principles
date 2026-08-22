using System.Net;
using System.Net.Sockets;

namespace Principles.Core.Services;

public static class ConnectivityExceptionClassifier
{
    public static bool IsConnectivityFailure( Exception exception )
    {
        foreach (Exception ex in Flatten( exception ))
        {
            if (ex is HttpRequestException || ex is WebException || ex is SocketException)
            {
                return true;
            }
        }

        return false;
    }

    private static IEnumerable<Exception> Flatten( Exception exception )
    {
        yield return exception;

        if (exception is AggregateException aggregateException)
        {
            foreach (Exception inner in aggregateException.Flatten().InnerExceptions.SelectMany( Flatten ))
            {
                yield return inner;
            }

            yield break;
        }

        if (exception.InnerException is not null)
        {
            foreach (Exception inner in Flatten( exception.InnerException ))
            {
                yield return inner;
            }
        }
    }
}
