using System.Linq.Expressions;

namespace Principles.Core.Services;

public interface IOfflineRepository
{
    Task<TField> GetFieldAsync<T, TField>( long localId, string fieldName );
    TField GetField<T, TField>( long localId, string fieldName );
    void UpdateField<T, TField>( long localId, string field, TField value ) where T : IOfflineEntity, new();
    Task UpdateFieldAsync<T, TField>( long localId, string field, TField value ) where T : IOfflineEntity, new();
    Task<T> GetRequiredByIdAsync<T>( long id ) where T : IOfflineEntity, new();
    Task<int> SaveRangeAsync<T>( IEnumerable<T> items ) where T : IOfflineEntity, new();
    Task<int> SaveAsync<T>(T entity) where T : IOfflineEntity, new();
    int Insert<T>(T entity) where T : IOfflineEntity, new();
    Task<int> InsertAsync<T>(T entity) where T : IOfflineEntity, new();
    int InsertRange<T>(IEnumerable<T> entities, bool runInTransaction = true) where T : IOfflineEntity, new();
    Task<int> InsertRangeAsync<T>(IEnumerable<T> entities, bool runInTransaction = true) where T : IOfflineEntity, new();
    int Update<T>(T entity) where T : IOfflineEntity, new();
    Task<int> UpdateAsync<T>(T entity) where T : IOfflineEntity, new();
    int UpdateRange<T>(IEnumerable<T> entities) where T : IOfflineEntity, new();
    Task<int> UpdateRangeAsync<T>(IEnumerable<T> entities) where T : IOfflineEntity, new();
    int Delete<T>(T entity) where T : IOfflineEntity, new();
    Task<int> DeleteAsync<T>(T entity) where T : IOfflineEntity, new();
    int DeleteById<T>(long pk) where T : IOfflineEntity, new();
    Task<int> DeleteByIdAsync<T>(long pk) where T : IOfflineEntity, new();
    T? GetById<T>(long pk) where T : IOfflineEntity, new();
    Task<T?> GetByIdAsync<T>(long pk) where T : IOfflineEntity, new();
    List<T> GetAll<T>() where T : IOfflineEntity, new();
    Task<List<T>> GetAllAsync<T>() where T : IOfflineEntity, new();
    List<T> Where<T>(Expression<Func<T, bool>> predicate) where T : IOfflineEntity, new();
    Task<List<T>> WhereAsync<T>(Expression<Func<T, bool>> predicate) where T : IOfflineEntity, new();
    int Count<T>(Expression<Func<T, bool>>? predicate = null) where T : IOfflineEntity, new();
    Task<int> CountAsync<T>(Expression<Func<T, bool>>? predicate = null) where T : IOfflineEntity, new();
    bool Exists<T>(Expression<Func<T, bool>> predicate) where T : IOfflineEntity, new();
    Task<bool> ExistsAsync<T>(Expression<Func<T, bool>> predicate) where T : IOfflineEntity, new();
    int Upsert<T>(T entity) where T : IOfflineEntity, new();
    Task<int> UpsertAsync<T>(T entity) where T : IOfflineEntity, new();
    void RunInTransaction(Action work);
    Task RunInTransactionAsync(Action work);
    List<T> Query<T>(string sql, params object[] args) where T : IOfflineEntity, new();
    Task<List<T>> QueryAsync<T>(string sql, params object[] args) where T : new();
    int Execute(string sql, params object[] args);
    Task<int> ExecuteAsync(string sql, params object[] args);
    void Dispose();
}