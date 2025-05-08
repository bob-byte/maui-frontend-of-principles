using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Principles.Core.Models;

namespace Principles.Core.Services;
public interface IDatabaseService
{
    //Task SaveUserAsync( UserDto user );

    /// <summary>
    /// Initializes the database by creating necessary tables.
    /// Should be called on application startup.
    /// </summary>
    Task InitializeAsync();

    /// <summary>
    /// Inserts a new entity into the database.
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <param name="entity">The entity to insert.</param>
    /// <returns>The number of rows affected.</returns>
    Task<int> InsertAsync<T>( T entity ) where T : new();

    /// <summary>
    /// Updates an existing entity in the database.
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <param name="entity">The entity to update.</param>
    /// <returns>The number of rows affected.</returns>
    Task<int> UpdateAsync<T>( T entity ) where T : new();

    /// <summary>
    /// Deletes an entity from the database.
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <param name="entity">The entity to delete.</param>
    /// <returns>The number of rows affected.</returns>
    Task<int> DeleteAsync<T>( T entity ) where T : new();

    /// <summary>
    /// Retrieves a single entity by its primary key.
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <param name="id">The primary key value.</param>
    /// <returns>The entity if found; otherwise, null.</returns>
    Task<T> GetByIdAsync<T>( object id ) where T : new();

    /// <summary>
    /// Retrieves all records of the specified entity type.
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <returns>A list of all entities of type T.</returns>
    Task<List<T>> GetAllAsync<T>() where T : new();

    /// <summary>
    /// Executes a raw SQL SELECT query and maps the results to a list of entities.
    /// </summary>
    /// <typeparam name="T">The type to map the results to.</typeparam>
    /// <param name="query">The raw SQL query.</param>
    /// <param name="args">Query parameters.</param>
    /// <returns>A list of entities of type T.</returns>
    Task<List<T>> QueryAsync<T>( string query, params object[] args ) where T : new();

    /// <summary>
    /// Executes a raw SQL command (INSERT, UPDATE, DELETE, etc.) that does not return results.
    /// </summary>
    /// <param name="query">The raw SQL command.</param>
    /// <param name="args">Command parameters.</param>
    /// <returns>The number of rows affected.</returns>
    Task<int> ExecuteAsync( string query, params object[] args );

    /// <summary>
    /// Retrieves entities that match the specified LINQ predicate.
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <param name="predicate">The LINQ predicate expression.</param>
    /// <returns>A list of matching entities.</returns>
    Task<List<T>> WhereAsync<T>( Expression<Func<T, bool>> predicate ) where T : new();

    /// <summary>
    /// Gets all UserInfo records from the database.
    /// </summary>
    /// <returns>A list of UserInfo objects.</returns>

}
