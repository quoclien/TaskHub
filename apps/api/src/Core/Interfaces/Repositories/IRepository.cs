using System.Linq.Expressions;
using TaskHub.Api.Core.Interfaces;
using TaskHub.Api.Core.Models.Pagination;

namespace TaskHub.Api.Core.Interfaces.Repositories
{
    /// <summary>
    /// Generic repository interface for data access operations
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    public interface IRepository<T> where T : class
    {
        /// <summary>
        /// Gets the unit of work instance for managing transactions
        /// </summary>
        IUnitOfWork UnitOfWork { get; }
        
        /// <summary>
        /// Retrieves an entity by its ID
        /// </summary>
        /// <param name="id">The ID of the entity to retrieve</param>
        /// <returns>The entity if found, otherwise null</returns>
        Task<T?> GetByIdAsync(Guid id);
        
        /// <summary>
        /// Retrieves an entity by its ID, throwing an exception if not found
        /// </summary>
        /// <param name="id">The ID of the entity to retrieve</param>
        /// <returns>The entity if found</returns>
        /// <exception cref="KeyNotFoundException">Thrown when the entity is not found</exception>
        Task<T> GetByIdOrThrowAsync(Guid id);
        
        /// <summary>
        /// Retrieves all entities
        /// </summary>
        /// <returns>A collection of all entities</returns>
        Task<IEnumerable<T>> GetAllAsync();
        
        /// <summary>
        /// Retrieves entities that match the specified predicate
        /// </summary>
        /// <param name="predicate">A function to test each element for a condition</param>
        /// <returns>A collection of entities that match the predicate</returns>
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        
        /// <summary>
        /// Adds a new entity to the repository
        /// </summary>
        /// <param name="entity">The entity to add</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task AddAsync(T entity);
        
        /// <summary>
        /// Adds a collection of entities to the repository
        /// </summary>
        /// <param name="entities">The entities to add</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task AddRangeAsync(IEnumerable<T> entities);
        
        /// <summary>
        /// Updates an existing entity in the repository
        /// </summary>
        /// <param name="entity">The entity to update</param>
        void Update(T entity);
        
        /// <summary>
        /// Removes an entity from the repository
        /// </summary>
        /// <param name="entity">The entity to remove</param>
        void Remove(T entity);
        
        /// <summary>
        /// Removes an entity from the repository (alias for Remove)
        /// </summary>
        /// <param name="entity">The entity to remove</param>
        void Delete(T entity) => Remove(entity);
        
        /// <summary>
        /// Removes a collection of entities from the repository
        /// </summary>
        /// <param name="entities">The entities to remove</param>
        void RemoveRange(IEnumerable<T> entities);
        
        /// <summary>
        /// Determines whether any element of a sequence satisfies a condition
        /// </summary>
        /// <param name="predicate">A function to test each element for a condition</param>
        /// <returns>true if any elements in the source sequence pass the test in the specified predicate; otherwise, false</returns>
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
        
        /// <summary>
        /// Determines whether any element of a sequence satisfies a condition (alias for AnyAsync)
        /// </summary>
        /// <param name="predicate">A function to test each element for a condition</param>
        /// <returns>true if any elements in the source sequence pass the test in the specified predicate; otherwise, false</returns>
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate) => AnyAsync(predicate);
        
        /// <summary>
        /// Returns the number of elements in a sequence that satisfy a condition
        /// </summary>
        /// <param name="predicate">A function to test each element for a condition</param>
        /// <returns>The number of elements in the sequence that satisfy the condition</returns>
        Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
        
        /// <summary>
        /// Returns the first element of a sequence that satisfies a specified condition
        /// </summary>
        /// <param name="predicate">A function to test each element for a condition</param>
        /// <param name="tracking">Whether to track the entity (default: true)</param>
        /// <returns>The first element that satisfies the condition</returns>
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, bool tracking = true);
        
        /// <summary>
        /// Returns the only element of a sequence that satisfies a specified condition, or a default value if no such element exists;
        /// this method throws an exception if more than one element satisfies the condition.
        /// </summary>
        /// <param name="predicate">A function to test each element for a condition</param>
        /// <param name="tracking">Whether to track the entity (default: true)</param>
        /// <returns>The single element of the sequence that satisfies the condition, or null if no such element is found</returns>
        Task<T?> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate, bool tracking = true);
        
        /// <summary>
        /// Returns a paginated list of entities that satisfy a condition
        /// </summary>
        /// <param name="predicate">A function to test each element for a condition</param>
        /// <param name="pageNumber">The page number (1-based)</param>
        /// <param name="pageSize">The number of items per page</param>
        /// <param name="orderBy">A function to order the elements</param>
        /// <param name="descending">Whether to sort in descending order (default: false)</param>
        /// <returns>A paginated list of entities</returns>
        Task<IPaginatedList<T>> GetPaginatedListAsync(
            Expression<Func<T, bool>>? predicate = null,
            int pageNumber = 1,
            int pageSize = 10,
            Expression<Func<T, object>>? orderBy = null,
            bool descending = false);
        
        /// <summary>
        /// Loads a collection of related entities for the given entity
        /// </summary>
        /// <typeparam name="TProperty">The type of the related entities</typeparam>
        /// <param name="entity">The entity to load the collection for</param>
        /// <param name="navigationProperty">The navigation property to load</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task LoadCollectionAsync<TProperty>(
            T entity, 
            Expression<Func<T, IEnumerable<TProperty>>> navigationProperty) 
            where TProperty : class;
            
        /// <summary>
        /// Loads a reference navigation property for the given entity
        /// </summary>
        /// <typeparam name="TProperty">The type of the related entity</typeparam>
        /// <param name="entity">The entity to load the reference for</param>
        /// <param name="navigationProperty">The navigation property to load</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task LoadReferenceAsync<TProperty>(
            T entity, 
            Expression<Func<T, TProperty?>> navigationProperty) 
            where TProperty : class;
            
        /// <summary>
        /// Executes a raw SQL query against the database
        /// </summary>
        /// <param name="sql">The SQL query</param>
        /// <param name="parameters">The parameters for the query</param>
        /// <returns>A collection of entities returned by the query</returns>
        Task<IEnumerable<T>> FromSqlAsync(string sql, params object[] parameters);
    }
}
