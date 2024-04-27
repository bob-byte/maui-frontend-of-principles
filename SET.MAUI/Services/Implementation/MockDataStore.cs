using SET.Core.Models;

namespace SET.MAUI.Services
{
    public class MockDataStore : IDataStore<Item>
    {
        readonly List<Item> items;

        public MockDataStore()
        {
            DateTime baseDate = DateTime.Today;
            this.items = new List<Item>() {
            new Item { Id = Guid.NewGuid(), Text = "First item", Description="This is an item description.", StartTime = baseDate.AddHours(1), EndTime = baseDate.AddHours(2), Value=17.098 },
            new Item { Id = Guid.NewGuid(), Text = "Second item", Description="This is an item description.", StartTime = baseDate.AddHours(2), EndTime = baseDate.AddHours(4), Value=9.985 },
            new Item { Id = Guid.NewGuid(), Text = "Third item", Description="This is an item description.", StartTime = baseDate.AddHours(3), EndTime = baseDate.AddHours(5), Value=9.597},
            new Item { Id = Guid.NewGuid(), Text = "Fourth item", Description="This is an item description.", StartTime = baseDate.AddHours(5), EndTime = baseDate.AddHours(6), Value=9.834 },
            new Item { Id = Guid.NewGuid(), Text = "Fifth item", Description="This is an item description.", StartTime = baseDate.AddHours(9), EndTime = baseDate.AddHours(12), Value=3.287 },
            new Item { Id = Guid.NewGuid(), Text = "Sixth item", Description="This is an item description.", StartTime = baseDate.AddHours(12), EndTime = baseDate.AddHours(15), Value=81.2 }
        };
        }

        public async Task<bool> AddItemAsync( Item item )
        {
            this.items.Add( item );

            return await Task.FromResult( true );
        }

        public async Task<bool> UpdateItemAsync( Item item )
        {
            var oldItem = this.items.Where( ( Item arg ) => arg.Id == item.Id ).FirstOrDefault();
            this.items.Remove( oldItem );
            this.items.Add( item );

            return await Task.FromResult( true );
        }

        public async Task<bool> DeleteItemAsync( Guid id )
        {
            var oldItem = this.items.Where( ( Item arg ) => arg.Id == id ).FirstOrDefault();
            this.items.Remove( oldItem );

            return await Task.FromResult( true );
        }

        public async Task<Item> GetItemAsync( Guid id )
        {
            return await Task.FromResult( this.items.FirstOrDefault( s => s.Id == id ) );
        }

        public async Task<IEnumerable<Item>> GetItemsAsync( bool forceRefresh = false )
        {
            return await Task.FromResult( this.items );
        }
    }

    public interface IDataStore<T>
    {
    }
}