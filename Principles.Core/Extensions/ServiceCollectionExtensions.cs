using Principles.Core.Services.AiKey;

using Microsoft.Extensions.DependencyInjection.Extensions;

using System.Net.Http.Headers;
using System.Net.Mime;

namespace Principles.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterAppCore( this IServiceCollection services )
    {
        // Platform-backed services (Preferences, SecureStorage, FileSystem, auth UI) are registered
        // from the MAUI app via RegisterMauiServices / TryAdd so unit tests can stub them first.
        services.TryAddSingleton<ISyncStateNotifier, SyncStateNotifier>();
        services.TryAddSingleton<IServiceOfTask, ServiceOfTask>();

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

        services.AddSingleton<IUrlBuilder, UrlBuilder>();
        services.AddSingleton<IRequestProvider, RequestProvider>();
        services.AddSingleton<IAreaOfLifeService, AreaOfLifeService>();
        services.AddSingleton<ILoginService, LoginService>();
        services.AddSingleton<IProgressOfHabitService, ProgressOfHabitService>();
        services.AddSingleton<IServiceOfHabit, ServiceOfHabit>();
        services.AddSingleton<IAiChatService, AiChatService>();
        services.AddSingleton<IAiRecommenderOfHabitsService, AiRecommenderOfHabitsService>();
        services.AddSingleton<IServiceLocator, ServiceLocator>();
        services.AddSingleton<IChangePasswordService, ChangePasswordService>();
        services.AddSingleton<ISignupService, SignupService>();
        services.AddSingleton<IGoalService, GoalService>();
        services.AddSingleton<IApiKeyService, ApiKeyService>();
        services.AddSingleton<ISyncService, SyncService>();
        services.AddSingleton<ISyncReachabilityService, SyncReachabilityService>();
        services.AddSingleton<ISyncOrchestrator, SyncOrchestrator>();
        services.AddSingleton<ISyncSnapshotRemoteApi, SyncSnapshotRemoteApi>();
        services.AddSingleton<ISyncSnapshotMergeService, SyncSnapshotMergeService>();
        services.AddSingleton<ISyncRetryConfig, DefaultSyncRetryConfig>();
        services.AddSingleton<ISyncQueueService, SyncQueueService>();
        services.AddSingleton<IDatabaseConnectionProvider, DatabaseConnectionProvider>();
        services.AddSingleton<IDatabaseMigrator, DatabaseMigrator>();
        services.AddSingleton<IDatabase, Database>();
        services.AddSingleton<ILocalRemoteExecutor, LocalRemoteExecutor>();
        services.AddSingleton<ServiceOfHabit>( sp => (ServiceOfHabit)sp.GetRequiredService<IServiceOfHabit>() );
        services.AddSingleton<RemoteApiService<User>, UserRemoteApi>();
        services.AddSingleton<RemoteApiService<UserGoal>, GoalRemoteApi>();
        services.AddSingleton<RemoteApiService<UserHabit>, HabitRemoteApi>();
        services.AddSingleton<RemoteApiService<ProgressOfHabit>, ProgressOfHabitRemoteApi>();
        services.AddSingleton<RemoteApiService<Reminder>, ReminderRemoteApi>();
        services.AddSingleton<IUserRemoteApi, UserRemoteApi>();
        services.AddSingleton<IUserService, UserService>();
        services.AddSingleton<IGoalRemoteApi, GoalRemoteApi>();
        services.AddSingleton<IHabitRemoteApi, HabitRemoteApi>();
        services.AddSingleton<IProgressOfHabitRemoteApi, ProgressOfHabitRemoteApi>();
        services.AddSingleton<IReminderRemoteApi, ReminderRemoteApi>();

        return services;
    }
}
