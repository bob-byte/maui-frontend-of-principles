using System.Net.Http.Headers;
using System.Net.Mime;

namespace SET.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterAppCore( this IServiceCollection services )
    {
        services.AddSingleton<ILoggingService, LoggingService>();

        services.
            AddHttpClient( nameof( RequestProvider ) ).
            ConfigureHttpClient(httpClient =>
            {
                httpClient.Timeout = TimeSpan.FromSeconds( value: 30 );
                httpClient.DefaultRequestHeaders.Accept.Add( new MediaTypeWithQualityHeaderValue( MediaTypeNames.Text.Plain ) );
            } );

        services.AddSingleton<ICachingService, CachingService>();
        services.AddSingleton<IUrlBuilder, UrlBuilder>();
        services.AddSingleton<IRequestProvider, RequestProvider>();
        services.AddSingleton<IServiceOfHabit, ServiceOfHabit>();
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

        return services;
    }
}
