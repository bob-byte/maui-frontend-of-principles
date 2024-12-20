using Azure.AI.OpenAI;

using Principles.Core.Constants;
using Principles.Core.Services.AiKey;

namespace Principles.Core.Services;

public class BaseRemoteService
{
    private readonly Lazy<OpenAIClient> m_lazyOpenAiClient;
    protected const string DEFAULT_AI_DEPLOYMENT_NAME = "gpt-4o-mini";

    public BaseRemoteService( IServiceProvider serviceProvider )
    {
        RequestProvider = serviceProvider.GetRequiredService<IRequestProvider>();
        LoggingService = serviceProvider.GetRequiredService<ILoggingService>();
        SettingsService = serviceProvider.GetRequiredService<ISettingsService>();
        ApiKeyService = serviceProvider.GetRequiredService<IApiKeyService>();

        m_lazyOpenAiClient = new(
            valueFactory: () =>
            {
                string? apiKey = SecureStorage.GetAsync( CacheKeys.API_KEY ).GetAwaiter().GetResult();
                if (string.IsNullOrEmpty( apiKey )) 
                {
                    apiKey  = ApiKeyService.GetApiKeyAsync().GetAwaiter().GetResult();
                    SecureStorage.SetAsync( CacheKeys.API_KEY, apiKey ).GetAwaiter().GetResult();
                }

                OpenAIClientOptions options = new( OpenAIClientOptions.ServiceVersion.V2023_09_01_Preview );
                OpenAIClient client = new( apiKey, options );
                return client;
            },
            mode: LazyThreadSafetyMode.ExecutionAndPublication
        );
    }

    protected IRequestProvider RequestProvider { get; }
    protected ILoggingService LoggingService { get; }
    protected ISettingsService SettingsService { get; }
    protected IApiKeyService ApiKeyService { get; }
    protected IUrlBuilder UrlBuilder => RequestProvider.UrlBuilder;

    protected OpenAIClient AiClient => m_lazyOpenAiClient.Value;
}
