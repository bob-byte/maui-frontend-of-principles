using Microsoft.Extensions.DependencyInjection;

using Serilog;

namespace SET.Tests.Unit;

public class Config
{
    static Config()
    {
        SetupSerilog();

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<ISettingsService, SimpleSettingsService>();
        serviceCollection.RegisterAppCore();

        ServiceProvider = serviceCollection.BuildServiceProvider();
    }

    public static IServiceProvider ServiceProvider { get; }

    private static void SetupSerilog()
    {
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .CreateLogger();
    }
}
