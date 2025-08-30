using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaskHub.Api.Core.Interfaces;
using TaskHub.Api.Core.Interfaces.Repositories;
using TaskHub.Api.Models.Entities;
using TaskHub.Api.Core.Models.Pagination;
using System.Data;
using Microsoft.Data.SqlClient;

namespace TaskHub.Api.Infrastructure.Data.Repositories
{
    /// <summary>
    /// Generic repository implementation for data access operations
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    public class Repository<T> : IRepository<T> where T : BaseEntity
    {
        protected readonly ApplicationDbContext _context;
        private readonly DbSet<T> _dbSet;

        /// <summary>
        /// Gets the unit of work instance for managing transactions
        /// </summary>
        public IUnitOfWork UnitOfWork { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Repository{T}"/> class
        /// </summary>
        /// <param name="unitOfWork">The unit of work instance</param>
        public Repository(IUnitOfWork unitOfWork)
        {
            UnitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _context = unitOfWork.Context ?? throw new ArgumentNullException(nameof(unitOfWork.Context));
            _dbSet = _context.Set<T>();
        }

        public async Task<T?> GetByIdAsync(Guid id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<T> GetByIdOrThrowAsync(Guid id)
        {
            var entity = await _dbSet.FindAsync(id);
            return entity ?? throw new KeyNotFoundException($"Entity of type {typeof(T).Name} with ID {id} not found.");
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public async Task AddRangeAsync(IEnumerable<T> entities)
        {
            await _dbSet.AddRangeAsync(entities);
        }

        public void Update(T entity)
        {
            _dbSet.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
        }

        public void Remove(T entity)
        {
            _dbSet.Remove(entity);
        }

        public void RemoveRange(IEnumerable<T> entities)
        {
            _dbSet.RemoveRange(entities);
        }

        public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }

        public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
        {
            return predicate == null
                ? await _dbSet.CountAsync()
                : await _dbSet.CountAsync(predicate);
        }

        /// <inheritdoc />
        public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, bool tracking = true)
        {
            var query = _dbSet.AsQueryable();

            if (!tracking)
            {
                query = query.AsNoTracking();
            }

            return await query.FirstOrDefaultAsync(predicate);
        }

        /// <inheritdoc />
        public async Task<T?> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate, bool tracking = true)
        {
            var query = _dbSet.AsQueryable();

            if (!tracking)
            {
                query = query.AsNoTracking();
            }

            return await query.SingleOrDefaultAsync(predicate);
        }

        /// <inheritdoc />
        public async Task<IPaginatedList<T>> GetPaginatedListAsync(
            Expression<Func<T, bool>>? predicate = null,
            int pageNumber = 1,
            int pageSize = 10,
            Expression<Func<T, object>>? orderBy = null,
            bool descending = false)
        {
            if (pageNumber < 1)
                throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number must be greater than 0.");

            if (pageSize < 1)
                throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than 0.");

            var query = _dbSet.AsQueryable();

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            var totalCount = await query.CountAsync();

            if (orderBy != null)
            {
                query = descending
                    ? query.OrderByDescending(orderBy)
                    : query.OrderBy(orderBy);
            }
            else
            {
                // Default ordering by ID if none specified
                query = query.OrderBy(e => e.Id);
            }

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedList<T>(items, totalCount, pageNumber, pageSize);
        }

        /// <inheritdoc />
        public async Task LoadCollectionAsync<TProperty>(
            T entity,
            Expression<Func<T, IEnumerable<TProperty>>> navigationProperty)
            where TProperty : class
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (navigationProperty == null)
                throw new ArgumentNullException(nameof(navigationProperty));

            var entry = _context.Entry(entity);
            if (entry.State == EntityState.Detached)
            {
                throw new InvalidOperationException("Cannot load collection for a detached entity.");
            }

            await entry.Collection(navigationProperty).LoadAsync();
        }

        /// <inheritdoc />
        public async Task LoadReferenceAsync<TProperty>(
            T entity,
            Expression<Func<T, TProperty?>> navigationProperty)
            where TProperty : class
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (navigationProperty == null)
                throw new ArgumentNullException(nameof(navigationProperty));

            var entry = _context.Entry(entity);
            if (entry.State == EntityState.Detached)
            {
                throw new InvalidOperationException("Cannot load reference for a detached entity.");
            }

            await entry.Reference(navigationProperty).LoadAsync();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<T>> FromSqlAsync(string sql, params object[] parameters)
        {
            if (string.IsNullOrWhiteSpace(sql))
                throw new ArgumentException("SQL query cannot be null or whitespace.", nameof(sql));

            // Convert object parameters to SqlParameter to prevent SQL injection
            var sqlParameters = parameters.Select((p, i) =>
                p is SqlParameter param
                    ? param
                    : new SqlParameter($"@p{i}", p ?? DBNull.Value)).ToArray();

            return await _dbSet.FromSqlRaw(sql, sqlParameters).ToListAsync();
        }
    }
}
