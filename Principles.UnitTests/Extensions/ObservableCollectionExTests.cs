using System.Collections.Specialized;

namespace Principles.UnitTests.Extensions;

public class ObservableCollectionExTests
{
    [Fact]
    public void Reload_ReplacesItemsAndRaisesReset()
    {
        ObservableCollectionEx<int> collection = [1, 2];
        NotifyCollectionChangedAction? action = null;
        collection.CollectionChanged += ( _, e ) => action = e.Action;

        collection.Reload( [3, 4, 5] );

        collection.Should().Equal( 3, 4, 5 );
        action.Should().Be( NotifyCollectionChangedAction.Reset );
    }

    [Fact]
    public void AddRange_AppendsAllItems()
    {
        ObservableCollectionEx<string> collection = ["a"];

        collection.AddRange( ["b", "c"] );

        collection.Should().Equal( "a", "b", "c" );
    }

    [Fact]
    public async Task ReloadAsync_ReplacesItems()
    {
        ObservableCollectionEx<int> collection = [9];

        await collection.ReloadAsync( async items =>
        {
            await Task.Yield();
            items.Add( 1 );
            items.Add( 2 );
        } );

        collection.Should().Equal( 1, 2 );
    }
}
