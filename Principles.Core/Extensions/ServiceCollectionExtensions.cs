using Principles.Core.Services.AiKey;

using System.Net.Http.Headers;
using System.Net.Mime;

namespace Principles.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterAppCore( this IServiceCollection services )
    {
        services.AddSingleton<ILoggingService, LoggingService>();

#if LOCALDEBUG
        services.AddHttpClient(nameof(RequestProvider))
                .ConfigureHttpClient(httpClient =>
                {
                    httpClient.Timeout = TimeSpan.FromSeconds(30);
                    httpClient.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue(MediaTypeNames.Text.Plain));
                })
                .ConfigurePrimaryHttpMessageHandler(() =>
                {
                    var handler = new HttpClientHandler();
                    handler.ServerCertificateCustomValidationCallback = ( message, cert, chain, errors ) =>
                    {
                        if (cert != null && cert.Issuer.Equals("CN=localhost"))
                        {
                            return true;
                        }

                        return errors == System.Net.Security.SslPolicyErrors.None;
                    };
                    return handler;
                });
#else
        services.AddHttpClient( nameof( RequestProvider ) )
                .ConfigureHttpClient( httpClient =>
                {
                    httpClient.Timeout = TimeSpan.FromSeconds( value: 30 );
                    httpClient.DefaultRequestHeaders.Accept.Add( new MediaTypeWithQualityHeaderValue( MediaTypeNames.Text.Plain ) );
                } );
#endif

        services.AddSingleton<ICachingService, CachingService>();
        services.AddSingleton<IUrlBuilder, UrlBuilder>();
        services.AddSingleton<IRequestProvider, RequestProvider>();
        services.AddSingleton<IAreaOfLifeService, AreaOfLifeService>();
        services.AddSingleton<ILoginService, LoginService>();
        services.AddSingleton<IProgressOfHabitService, ProgressOfHabitService>();
        services.AddSingleton<IAccountService, AccountService>();
        services.AddSingleton<IAiChatService, AiChatService>();
        services.AddSingleton<IAiRecommenderOfHabitsService, AiRecommenderOfHabitsService>();
        services.AddSingleton<IServiceLocator, ServiceLocator>();
        services.AddSingleton<IChangePasswordService, ChangePasswordService>();
        services.AddSingleton<ISignupService, SignupService>();
        services.AddSingleton<IGoalService, GoalService>();
        services.AddSingleton<IGoogleAuthService, GoogleAuthService>();
        services.AddSingleton<IVersionCheckerService, VersionCheckerService>();
        services.AddSingleton<IAppleAuthService, AppleAuthService>();
        services.AddSingleton<IApiKeyService, ApiKeyService>();

        services.AddSingleton<ISyncService, SyncService>();
        services.AddSingleton<ISyncRetryConfig, DefaultSyncRetryConfig>();
        services.AddSingleton<ISyncQueueService, SyncQueueService>();
        services.AddSingleton<IDatabaseConnectionProvider, DatabaseConnectionProvider>();
        services.AddSingleton<IDatabaseMigrator, DatabaseMigrator>();

        services.AddSingleton<ILocalRemoteExecutor, LocalRemoteExecutor>();

        services.AddSingleton<RemoteApiService<User>, UserRemoteApi>();
        services.AddSingleton<IUserRemoteApi, UserRemoteApi>();
        services.AddSingleton<IUserService, UserService>();

        services.AddSingleton<RemoteApiService<UserHabit>, HabitRemoteApi>();
        services.AddSingleton<IHabitRemoteApi, HabitRemoteApi>();
        services.AddSingleton<IServiceOfHabit, ServiceOfHabit>();

        services.AddSingleton<IProgressOfHabitRemoteApi, ProgressOfHabitRemoteApi>();
        services.AddSingleton<RemoteApiService<ProgressOfHabit>, ProgressOfHabitRemoteApi>();

        return services;
    }
}
