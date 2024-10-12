namespace Principles.Core.Services;

public interface IServiceLocator
{
    IServiceProvider ServiceProvider { get; }

    T GetRequiredService<T>() where T : class;

    object GetRequiredService( Type type );

    T? GetService<T>() where T : class;

    object? GetService( Type type );

    void RegisterSingleton<T>( T service ) where T : class;

    bool IsServiceRegistered<T>() where T : class;
}
