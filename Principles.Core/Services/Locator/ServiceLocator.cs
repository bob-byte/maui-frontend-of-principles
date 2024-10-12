using System.Collections.Concurrent;

namespace Principles.Core.Services;

public class ServiceLocator : IServiceLocator
{
    private readonly ConcurrentDictionary<Type, object> m_dynamicallyAddedServices;

    public ServiceLocator( IServiceProvider serviceProvider )
    {
        m_dynamicallyAddedServices = new ConcurrentDictionary<Type, object>();
        ServiceProvider = serviceProvider;
    }

    public static IServiceLocator? Current => GetCurrentLocator?.Invoke();

    public static Func<IServiceLocator>? GetCurrentLocator { get; set; }

    public IServiceProvider ServiceProvider { get; }

    public T GetRequiredService<T>() where T : class
    {
        T? result = GetService<T>();
        if (result == null)
        {
            throw new InvalidOperationException($"Service '{typeof(T).FullName}' not registered");
        }
        else
        {
            return result;
        }
    }

    public object GetRequiredService(Type type)
    {
        object? result = GetService( type );
        if (result == null)
        {
            throw new InvalidOperationException(message: $"Service '{type}' not registered");
        }

        return result!;
    }

    public object? GetService( Type type )
    {
        object? result = ServiceProvider.GetService( type );
        if (result == null)
        {
            m_dynamicallyAddedServices.TryGetValue( key: type, out result );
        }

        return result;
    }

    public T? GetService<T>() where T : class
    {
        object? result = ServiceProvider.GetService<T>();
        if (result == null)
        {
            m_dynamicallyAddedServices.TryGetValue(key: typeof(T), out result);
        }

        return result as T;
    }

    public void RegisterSingleton<T>(T service) where T : class
    {
        if (!IsServiceRegistered<T>())
        {
            m_dynamicallyAddedServices.TryAdd(key: typeof(T), service);
        }
    }

    public bool IsServiceRegistered<T>() where T : class
    {
        return ServiceProvider.GetService<T>() != null || m_dynamicallyAddedServices.ContainsKey(key: typeof(T));
    }
}
