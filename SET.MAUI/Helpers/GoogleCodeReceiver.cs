using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Requests;
using Google.Apis.Auth.OAuth2.Responses;

namespace SET.MAUI.Helpers;

public class GoogleCodeReceiver : ICodeReceiver
{
    public string RedirectUri => new LaunchUriBuilder( LaunchType.OAuth2Redirect ).Build().AbsoluteUri;

    public async Task<AuthorizationCodeResponseUrl> ReceiveCodeAsync(
        AuthorizationCodeRequestUrl url,
        CancellationToken taskCancellationToken )
    {
        string urlToOpen = url.Build().AbsoluteUri;
        await Launcher.Default.OpenAsync( urlToOpen ).DefaultConfigureAwait();

        LaunchResult result = await LaunchUriHelper.LaunchResult.DefaultConfigureAwait();
        LaunchUriHelper.Reset();

        if (result.Type == LaunchType.OAuth2Redirect)
        {
            AuthorizationCodeResponseUrl response = new()
            {
                Code = result.Query.Get( "code" ),
                State = result.Query.Get( "state" ),
                Error = result.Query.Get( "error" ),
                ErrorDescription = result.Query.Get( "error_description" ),
                ErrorUri = result.Query.Get( "error_uri" )
            };

            return response;
        }
        else
        {
            throw new InvalidOperationException( message: $"Invalid type of launch result: {result.Type}" );
        }
    }
}
