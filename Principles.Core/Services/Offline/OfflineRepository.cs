using System.Linq.Expressions;

namespace Principles.Core.Services;

public class OfflineRepository : IOfflineRepository
{
    private readonly SQLiteConnection m_syncConn;
    private readonly SQLiteAsyncConnection m_asyncConn;
    private bool m_disposed;

    public OfflineRepository(string dbPath)
    {
        SQLiteOpenFlags flags = SQLiteOpenFlags.ReadWrite |
                                SQLiteOpenFlags.Create |
                                SQLiteOpenFlags.SharedCache |
                                SQLiteOpenFlags.FullMutex;

        m_syncConn = new SQLiteConnection( dbPath, flags );
        m_asyncConn =
            new SQLiteAsyncConnection( dbPath, flags );
        
        Type interfaceType = typeof(IOfflineEntity);

        // Find all types that implement IOfflineEntity
        Type[] tableTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => interfaceType.IsAssignableFrom(t) && t is { IsInterface: false, IsAbstract: false })
            .ToArray();

        m_syncConn.CreateTables(CreateFlags.None, tableTypes);
    }
    
    public Task<TField> GetFieldAsync<T, TField>( long localId, string fieldName )
    { 
        return m_asyncConn.ExecuteScalarAsync<TField>( $"SELECT {fieldName} FROM {typeof(T).Name} WHERE LocalId = ?;", localId);
    }
    
    public TField GetField<T, TField>( long localId, string fieldName )
    { 
        return m_syncConn.ExecuteScalar<TField>( $"SELECT {fieldName} FROM {typeof(T).Name} WHERE LocalId = ?;", localId);
    }
    
    public void UpdateField<T, TField>( long localId, string field, TField value ) where T : IOfflineEntity, new()
    {
        m_syncConn.Execute( $"UPDATE {typeof(T).Name} SET {field} = {value} WHERE LocalId = {localId};" );
    }
    
    public Task UpdateFieldAsync<T, TField>( long localId, string field, TField value ) where T : IOfflineEntity, new()
    {
        return m_asyncConn.ExecuteAsync( $"UPDATE {typeof(T).Name} SET {field} = {value} WHERE LocalId = {localId};" );
    }

    public async Task<T> GetRequiredByIdAsync<T>( long id ) where T : IOfflineEntity, new()
    {
        T? result = await GetByIdAsync<T>(id);
        if (result is null)
        {
            throw new KeyNotFoundException("The requested offline entity does not exist.");
        }
        else
        {
            return result;
        }
    }

    public async Task<int> SaveRangeAsync<T>( IEnumerable<T> items ) where T : IOfflineEntity, new()
    {
        var list  = items.ToList();
        
        IEnumerable<T> newItems = list.Where(t => t.LocalId == 0);
        int result = await InsertRangeAsync(newItems);
        
        IEnumerable<T> oldItems = list.Where(t => t.LocalId != 0);
        result += await UpdateRangeAsync( oldItems );
        
        return result;
    }

    public Task<int> SaveAsync<T>(T entity) where T : IOfflineEntity, new()
    {
        long id = entity.LocalId;
            
        // Check if entity already exists
        if (id == 0)
        {
            return InsertAsync( entity );
        }
        else
        {
            return UpdateAsync( entity );
        }
    }

    // ----------------- INSERT -----------------
    public int Insert<T>(T entity) where T : IOfflineEntity, new()
    {
        return m_syncConn.Insert( entity );
    }

    public Task<int> InsertAsync<T>(T entity) where T : IOfflineEntity, new()
    {
        return m_asyncConn.InsertAsync( entity );
    }

    public int InsertRange<T>(IEnumerable<T> entities, bool runInTransaction = true) where T : IOfflineEntity, new()
    {
        return m_syncConn.InsertAll( entities, runInTransaction );
    }

    public Task<int> InsertRangeAsync<T>(IEnumerable<T> entities, bool runInTransaction = true) where T : IOfflineEntity, new()
    {
        return m_asyncConn.InsertAllAsync( entities, runInTransaction );
    }

    // ----------------- UPDATE -----------------
    public int Update<T>(T entity) where T : IOfflineEntity, new()
    {
        return m_syncConn.Update( entity );
    }

    public Task<int> UpdateAsync<T>(T entity) where T : IOfflineEntity, new()
    {
        return m_asyncConn.UpdateAsync( entity );
    }

    public int UpdateRange<T>(IEnumerable<T> entities) where T : IOfflineEntity, new()
    {
        return m_syncConn.UpdateAll( entities );
    }

    public Task<int> UpdateRangeAsync<T>(IEnumerable<T> entities) where T : IOfflineEntity, new()
    {
        return m_asyncConn.UpdateAllAsync( entities );
    }

    // ----------------- DELETE -----------------
    public int Delete<T>(T entity) where T : IOfflineEntity, new()
    {
        return m_syncConn.Delete( entity );
    }

    public Task<int> DeleteAsync<T>(T entity) where T : IOfflineEntity, new()
    {
        return m_asyncConn.DeleteAsync( entity );
    }

    public int DeleteById<T>(long pk) where T : IOfflineEntity, new()
    {
        return m_syncConn.Delete<T>( pk );
    }

    public Task<int> DeleteByIdAsync<T>(long pk) where T : IOfflineEntity, new()
    {
        return m_asyncConn.DeleteAsync<T>( pk );
    }

    // ----------------- GET / FIND -----------------
    public T? GetById<T>(long pk) where T : IOfflineEntity, new()
    {
        return m_syncConn.Find<T>( pk );
    }

    public Task<T?> GetByIdAsync<T>(long pk) where T : IOfflineEntity, new()
    {
        return m_asyncConn.FindAsync<T?>( pk );
    }

    public List<T> GetAll<T>() where T : IOfflineEntity, new()
    {
        return m_syncConn.Table<T>().ToList();
    }

    public Task<List<T>> GetAllAsync<T>() where T : IOfflineEntity, new()
    {
        return m_asyncConn.Table<T>().ToListAsync();
    }

    // ----------------- QUERY / FILTER -----------------
    public List<T> Where<T>(Expression<Func<T, bool>> predicate) where T : IOfflineEntity, new()
    {
        return m_syncConn.Table<T>().Where( predicate ).ToList();
    }

    public Task<List<T>> WhereAsync<T>(Expression<Func<T, bool>> predicate) where T : IOfflineEntity, new()
    {
        return m_asyncConn.Table<T>().Where( predicate ).ToListAsync();
    }

    // ----------------- COUNT / EXISTS -----------------
    public int Count<T>(Expression<Func<T, bool>>? predicate = null) where T : IOfflineEntity, new()
    {
        return predicate == null
            ? m_syncConn.Table<T>().Count()
            : m_syncConn.Table<T>().Where( predicate ).Count();
    }

    public Task<int> CountAsync<T>(Expression<Func<T, bool>>? predicate = null) where T : IOfflineEntity, new()
    {
        return predicate == null
            ? m_asyncConn.Table<T>().CountAsync()
            : m_asyncConn.Table<T>().Where( predicate ).CountAsync();
    }

    public bool Exists<T>(Expression<Func<T, bool>> predicate) where T : IOfflineEntity, new()
    {
        return m_syncConn.Table<T>().Where( predicate ).FirstOrDefault() != null;
    }

    public Task<bool> ExistsAsync<T>(Expression<Func<T, bool>> predicate) where T : IOfflineEntity, new()
    {
        return m_asyncConn.Table<T>().Where( predicate ).FirstOrDefaultAsync()
            .ContinueWith( t => t.Result != null );
    }

    // ----------------- UPSERT -----------------
    public int Upsert<T>(T entity) where T : IOfflineEntity, new()
    {
        return m_syncConn.InsertOrReplace( entity );
    }

    public Task<int> UpsertAsync<T>(T entity) where T : IOfflineEntity, new()
    {
        return m_asyncConn.InsertOrReplaceAsync( entity );
    }

    // ----------------- TRANSACTIONS -----------------
    public void RunInTransaction(Action work)
    {
        m_syncConn.RunInTransaction( work );
    }

    public Task RunInTransactionAsync(Action work)
    {
        return m_asyncConn.RunInTransactionAsync( _ => work() );
    }

    // ----------------- RAW SQL -----------------
    public List<T> Query<T>(string sql, params object[] args) where T : IOfflineEntity, new()
    {
        return m_syncConn.Query<T>( sql, args );
    }

    public Task<List<T>> QueryAsync<T>(string sql, params object[] args) where T : new()
    {
        return m_asyncConn.QueryAsync<T>( sql, args );
    }

    public int Execute(string sql, params object[] args)
    {
        return m_syncConn.Execute( sql, args );
    }

    public Task<int> ExecuteAsync(string sql, params object[] args)
    {
        return m_asyncConn.ExecuteAsync( sql, args );
    }

    // ----------------- CLEANUP -----------------
    public void Dispose()
    {
        if (m_disposed) return;
        m_disposed = true;
        m_syncConn?.Close();
        m_syncConn?.Dispose();
    }
}
