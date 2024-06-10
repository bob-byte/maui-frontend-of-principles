using Azure.AI.OpenAI;

namespace SET.Core.Services;

public class BaseRemoteService
{
    private readonly Lazy<OpenAIClient> m_lazyOpenAiClient;
    protected const string DEFAULT_AI_DEPLOYMENT_NAME = "gpt-3.5-turbo";

    public BaseRemoteService( IServiceProvider serviceProvider )
    {
        RequestProvider = serviceProvider.GetRequiredService<IRequestProvider>();
        LoggingService = serviceProvider.GetRequiredService<ILoggingService>();
        SettingsService = serviceProvider.GetRequiredService<ISettingsService>();

        m_lazyOpenAiClient = new(
            valueFactory: () =>
            {
                OpenAIClientOptions options = new( OpenAIClientOptions.ServiceVersion.V2023_09_01_Preview );
                OpenAIClient client = new( openAIApiKey: "", options );
                return client;
            },
            mode: LazyThreadSafetyMode.ExecutionAndPublication
        );
    }

    protected IRequestProvider RequestProvider { get; }
    protected ILoggingService LoggingService { get; }
    protected ISettingsService SettingsService { get; }
    protected IUrlBuilder UrlBuilder => RequestProvider.UrlBuilder;

    protected OpenAIClient AiClient => m_lazyOpenAiClient.Value;
}
