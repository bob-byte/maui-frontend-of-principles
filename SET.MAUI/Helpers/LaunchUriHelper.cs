using SET.Core.Models.Enums;

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SET.MAUI.Helpers;
public static class LaunchUriHelper
{
    private static readonly TaskCompletionSource<LaunchResult> OnReturned = new();

    public static Task<LaunchResult> LaunchResult => OnReturned.Task;

    public static bool TryHandle( Uri uri )
    {
        if (uri.Scheme != AppInfo.PackageName) return false;
        LaunchType type = uri.AbsolutePath == LaunchUriBuilder.RedirectPath ? LaunchType.OAuth2Redirect : LaunchType.Unknown;
        NameValueCollection query = HttpUtility.ParseQueryString( uri.Query );
        OnReturned.SetResult( new LaunchResult( type, query ) );
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
