using Microsoft.Extensions.DependencyInjection;

namespace Principles.UnitTests.Infrastructure;

/// <summary>
/// Builds <see cref="ServiceOfHabit"/> with substituted collaborators so pure logic
/// can be tested without opening SQLCipher or hitting the network.
/// </summary>
public static class ServiceOfHabitFactory
{
    public static ServiceOfHabit Create(
        out IReminderService reminderService,
        out INetworkService networkService,
        out IHabitRemoteApi habitRemoteApi )
    {
        reminderService = Substitute.For<IReminderService>();
        networkService = Substitute.For<INetworkService>();
        habitRemoteApi = Substitute.For<IHabitRemoteApi>();

        IServiceCollection services = new ServiceCollection();
        services.AddSingleton( Substitute.For<IRequestProvider>() );
        services.AddSingleton( Substitute.For<ILoggingService>() );
        services.AddSingleton<ISettingsService, SimpleSettingsService>();
        services.AddSingleton( Substitute.For<IDatabase>() );
        services.AddSingleton( Substitute.For<ISyncQueueService>() );
        services.AddSingleton( Substitute.For<ILocalRemoteExecutor>() );
        services.AddSingleton( reminderService );
        services.AddSingleton( networkService );
        services.AddSingleton( habitRemoteApi );

        return new ServiceOfHabit( services.BuildServiceProvider() );
    }

    public static ServiceOfHabit Create() =>
        Create( out _, out _, out _ );
}
