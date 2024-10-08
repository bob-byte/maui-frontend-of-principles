using System.Collections.Specialized;
using System.Web;

namespace Principles.Helpers;

public class LaunchUriHelper : ILaunchUriHelper
{
    private TaskCompletionSource<LaunchExternalOAuthResult> m_onReturned = new();

    public Task<LaunchExternalOAuthResult> LaunchResult => m_onReturned.Task;

    public bool TryHandle( Uri uri )
    {
        if (uri.Scheme != AppInfo.PackageName)
        {
            return false;
        }

        LaunchType type = uri.AbsolutePath == LaunchUriBuilder.REDIRECT_PATH
            ? LaunchType.OAuth2Redirect
            : LaunchType.Unknown;
        NameValueCollection query = HttpUtility.ParseQueryString( uri.Query );
        m_onReturned.SetResult( new LaunchExternalOAuthResult( type, query.Get( "code" ), query.Get( "error" ) ) );
        return true;
    }

    public void Reset()
    {
        if (!m_onReturned.Task.IsCompleted)
        {
            m_onReturned.SetCanceled();
        }

        m_onReturned = new();
    }
}
