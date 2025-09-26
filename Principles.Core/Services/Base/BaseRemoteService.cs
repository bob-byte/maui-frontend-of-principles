using Azure.AI.OpenAI;
using Azure.Core;
using Azure.Core.Pipeline;

using OpenAI;
using OpenAI.Chat;

using Principles.Core.Constants;
using Principles.Core.Services.AiKey;

using System;
using System.ClientModel;
using System.Threading;
using System.Threading.Tasks;

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

    protected static AsyncLazy<ChatClient> LazyAiClient { get; } = new(
        async () =>
        {
            IApiKeyService apiKeyService = ServiceLocator.Current!.GetRequiredService<IApiKeyService>();
            string apiKey = await apiKeyService.RestoreApiKeyAsync().ConfigureAwait( false );

            ChatClient client = new( DEFAULT_AI_DEPLOYMENT_NAME, apiKey );
            return client;
        },
        mode: LazyThreadSafetyMode.None
    );

    protected IRequestProvider RequestProvider { get; }
    protected ILoggingService LoggingService { get; }
    protected ISettingsService SettingsService { get; }
    protected IUrlBuilder UrlBuilder => RequestProvider.UrlBuilder;
}
