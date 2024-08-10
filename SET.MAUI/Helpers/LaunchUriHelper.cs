using System.Collections.Specialized;
using System.Web;

namespace SET.MAUI.Helpers;
public static class LaunchUriHelper
{
    private static TaskCompletionSource<LaunchResult> s_onReturned = new();

    public static Task<LaunchResult> LaunchResult => s_onReturned.Task;

    public static void Reset()
    {
        s_onReturned = new TaskCompletionSource<LaunchResult>();
    }

    public static bool TryHandle( Uri uri )
    {
        if (uri.Scheme != AppInfo.PackageName)
        {
            return false;
        }

        LaunchType type = uri.AbsolutePath == LaunchUriBuilder.REDIRECT_PATH 
            ? LaunchType.OAuth2Redirect 
            : LaunchType.Unknown;
        NameValueCollection query = HttpUtility.ParseQueryString( uri.Query );
        s_onReturned.SetResult( new LaunchResult( type, query ) );
        return true;
    }
}

public class LaunchResult
{
    public LaunchResult( LaunchType type, NameValueCollection query )
    {
        Type = type;
        Query = query;
    }

    public LaunchType Type { get; }
    public NameValueCollection Query { get; }
}
