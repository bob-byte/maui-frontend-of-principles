namespace SET.Core.Services;

public interface IServiceLocator
{
    IServiceProvider ServiceProvider { get; }

    T GetService<T>() where T : class;

    object GetService( Type type );

    T? GetServiceOrNull<T>() where T : class;

    object? GetServiceOrNull( Type type );

    void RegisterSingleton<T>( T service ) where T : class;

    bool IsServiceRegistered<T>() where T : class;
}
