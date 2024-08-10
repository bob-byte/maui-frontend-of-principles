using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;

namespace SET.Core.Services;

public interface IGoogleAuthService
{
    Task AuthorizeAsync( ICodeReceiver codeReceiver, IDataStore dataStore );
}
