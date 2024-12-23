namespace Principles.Core.Helpers;

public class AsyncLazy<T> : Lazy<Task<T>>
{
    //The Value property will be executed synchronously if you do not use Task.Factory.StartNew 
    public AsyncLazy( Func<Task<T>> taskFactory, LazyThreadSafetyMode mode ) 
        : base( () => Task.Factory.StartNew(taskFactory).Unwrap(), mode )
    {
        //do nothing
    }
    
    public TaskAwaiter<T> GetAwaiter()
    {
        return Value.GetAwaiter();
    }
}
