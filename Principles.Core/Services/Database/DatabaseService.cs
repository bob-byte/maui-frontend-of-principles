using SQLite;
using Principles.Core.Models;
using System.Linq.Expressions;

namespace Principles.Core.Services;
public class DatabaseService : IDatabaseService
{
    private readonly SQLiteAsyncConnection _db;

    public DatabaseService()
    {
        string dbPath = Path.Combine( FileSystem.AppDataDirectory, "app.db" );
        _db = new SQLiteAsyncConnection( dbPath );
        _ = InitializeAsync();
    }
    //public async Task SaveUserAsync( UserDto user )
    //{
    //    UserInfo localUser = new UserInfo
    //    {
    //        Name = user.Name,
    //        Email = user.Email,
    //        Mission = user.Mission ?? string.Empty,
    //        MainSlogan = user.MainSlogan ?? string.Empty,
    //        Gender = user.Gender,
    //        IsSynced = true // Оскільки приходить з сервера
    //    };

    //    await _db.DeleteAllAsync<UserInfo>(); 
    //    await _db.InsertAsync( localUser );
    //}
    public async Task InitializeAsync()
    {
        await _db.CreateTableAsync<UserInfo>();
    }

    public async Task<int> InsertAsync<T>( T entity ) where T : new()
    {
        return await _db.InsertAsync( entity );
    }

    public async Task<int> UpdateAsync<T>( T entity ) where T : new()
    {
        return await _db.UpdateAsync( entity );
    }

    public async Task<int> DeleteAsync<T>( T entity ) where T : new()
    {
        return await _db.DeleteAsync( entity );
    }

    public async Task<T> GetByIdAsync<T>( object id ) where T : new()
    {
        return await _db.FindAsync<T>( id );
    }

    public async Task<List<T>> GetAllAsync<T>() where T : new()
    {
        return await _db.Table<T>().ToListAsync();
    }

    public async Task<List<T>> QueryAsync<T>( string query, params object[] args ) where T : new()
    {
        return await _db.QueryAsync<T>( query, args );
    }

    public async Task<int> ExecuteAsync( string query, params object[] args )
    {
        return await _db.ExecuteAsync( query, args );
    }

    public async Task<List<T>> WhereAsync<T>( Expression<Func<T, bool>> predicate ) where T : new()
    {
        return await _db.Table<T>().Where( predicate ).ToListAsync();
    }
}
