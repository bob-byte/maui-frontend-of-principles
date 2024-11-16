namespace SET.Core.Extensions;

public static class IEnumerableExtensions
{
    public static ObservableCollection<T> ToObservableCollection<T>( this IEnumerable<T> source )
    {
        return new ObservableCollection<T>( source );
    }
    
    public static ObservableCollectionEx<T> ToExObservableCollection<T>( this IEnumerable<T> source )
    {
        return new ObservableCollectionEx<T>( source );
    }
}

