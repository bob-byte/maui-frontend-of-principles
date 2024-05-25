using SET.Core.Models;

namespace SET.MAUI.Services
{
    public class MockDataStore : IDataStore<Item>
    {
        readonly List<Item> items;

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

        public async Task<bool> DeleteItemAsync( long id )
        {
            var oldItem = this.items.Where( ( Item arg ) => arg.Id == id ).FirstOrDefault();
            this.items.Remove( oldItem );

            return await Task.FromResult( true );
        }

        public async Task<Item> GetItemAsync( long id )
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