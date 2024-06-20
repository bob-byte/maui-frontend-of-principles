using Polly;
using Polly.Extensions.Http;
using Polly.Retry;

using SET.Core.Services;

using System;
using System.Net;

namespace SET.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterAppCore( this IServiceCollection services )
    {
        services.AddSingleton<ILoggingService, LoggingService>();
        services.AddHttpClient<IRequestProvider, RequestProvider>().AddPolicyHandler( RetryPolicy() );
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

        return services;
    }

    private static AsyncRetryPolicy<HttpResponseMessage> RetryPolicy()
    {
        PolicyBuilder<HttpResponseMessage> policyBuilder = HttpPolicyExtensions.HandleTransientHttpError();
        return policyBuilder.WaitAndRetryAsync( retryCount: 5, ( numRetry ) => TimeSpan.FromSeconds( Math.Pow( 1.5, numRetry ) ) );
    }
}

