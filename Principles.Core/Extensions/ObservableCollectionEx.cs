using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Principles.Core.Extensions;

public class ObservableCollectionEx<T> : ObservableCollection<T>
{
    public ObservableCollectionEx() : base()
    {
    }

    public ObservableCollectionEx( IEnumerable<T> collection ) : base( collection )
    {
    }

    public ObservableCollectionEx( List<T> list ) : base( list )
    {
    }

    public void Reload( IEnumerable<T> items )
    {
        Reload(
            innerList =>
            {
                foreach (T item in items)
                {
                    innerList.Add( item );
                }
            } 
        );
    }

    public void Reload( Action<IList<T>> innerListAction )
    {
        Items.Clear();

        innerListAction( Items );

        OnPropertyChanged( new PropertyChangedEventArgs( nameof( Count ) ) );
        OnPropertyChanged( new PropertyChangedEventArgs( "Items[]" ) );
        OnCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset ) );
    }

    public async Task ReloadAsync( Func<IList<T>, Task> innerListAction )
    {
        Items.Clear();

        await innerListAction( Items );

        OnPropertyChanged( new PropertyChangedEventArgs( nameof( Count ) ) );
        OnPropertyChanged( new PropertyChangedEventArgs( "Items[]" ) );
        OnCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset ) );
    }
}

