
namespace Principles.Core;

public class LaunchUriBuilder
{
    public const string REDIRECT_PATH = "oauth2/redirect";

    private readonly LaunchType m_type;

    public LaunchUriBuilder( LaunchType type )
    {
        m_type = type;
    }

    public Uri Build()
    {
        switch (m_type)
        {
            case LaunchType.OAuth2Redirect:
                return new UriBuilder( AppInfo.PackageName, "" )
                {
                    Path = REDIRECT_PATH
                }.Uri;
            default:
                throw new NotSupportedException();
        }
    }
}
