using Azure.AI.OpenAI;

using Principles.Core.Constants;
using Principles.Core.Services.AiKey;

namespace Principles.Core.Services;

public class BaseRemoteService
{
    public BaseRemoteService( IServiceProvider serviceProvider )
    {
        RequestProvider = serviceProvider.GetRequiredService<IRequestProvider>();
        LoggingService = serviceProvider.GetRequiredService<ILoggingService>();
        SettingsService = serviceProvider.GetRequiredService<ISettingsService>();
    }
    
    protected const string DEFAULT_AI_DEPLOYMENT_NAME = "gpt-4o-mini";
    
    protected static AsyncLazy<OpenAIClient> LazyAiClient { get; } = new(
        async () =>
        {
            IApiKeyService apiKeyService = ServiceLocator.Current!.GetRequiredService<IApiKeyService>();
            string apiKey = await apiKeyService.RestoreApiKeyAsync().DefaultConfigureAwait();

            OpenAIClientOptions options = new( OpenAIClientOptions.ServiceVersion.V2023_09_01_Preview );
            OpenAIClient client = new( apiKey, options );
            return client;
        },
        mode: LazyThreadSafetyMode.None
    );

    protected IRequestProvider RequestProvider { get; }
    protected ILoggingService LoggingService { get; }
    protected ISettingsService SettingsService { get; }
    protected IUrlBuilder UrlBuilder => RequestProvider.UrlBuilder;
}
