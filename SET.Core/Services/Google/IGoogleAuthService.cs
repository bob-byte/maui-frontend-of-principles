using Google.Apis.Auth.OAuth2;

namespace SET.Core.Services;

public interface IGoogleAuthService
{
    Task AuthorizeAsync( ICodeReceiver codeReceiver );
}
