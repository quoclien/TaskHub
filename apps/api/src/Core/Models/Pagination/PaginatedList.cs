using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace TaskHub.Api.Core.Models.Pagination
{
    /// <summary>
    /// Represents a paginated list of items with pagination metadata
    /// </summary>
    /// <typeparam name="T">The type of items in the list</typeparam>
    public class PaginatedList<T> : IPaginatedList<T>
    {
        private readonly List<T> _items = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="PaginatedList{T}"/> class
        /// </summary>
        /// <param name="items">The items for the current page</param>
        /// <param name="totalCount">The total number of items across all pages</param>
        /// <param name="pageNumber">The current page number (1-based)</param>
        /// <param name="pageSize">The number of items per page</param>
        public PaginatedList(IEnumerable<T> items, int totalCount, int pageNumber, int pageSize)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));
                
            if (pageNumber < 1)
                throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number must be greater than 0.");
                
            if (pageSize < 1)
                throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than 0.");
                
            if (totalCount < 0)
                throw new ArgumentOutOfRangeException(nameof(totalCount), "Total count cannot be negative.");

            _items.AddRange(items);
            TotalCount = totalCount;
            PageNumber = pageNumber;
            PageSize = pageSize;
        }

        /// <inheritdoc />
        public int PageNumber { get; }

        /// <inheritdoc />
        public int PageSize { get; }

        /// <inheritdoc />
        public int TotalCount { get; }

        /// <inheritdoc />
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

        /// <inheritdoc />
        public bool HasPreviousPage => PageNumber > 1;

        /// <inheritdoc />
        public bool HasNextPage => PageNumber < TotalPages;

        /// <inheritdoc />
        public IReadOnlyCollection<T> Items => _items.AsReadOnly();

        /// <summary>
        /// Creates an empty paginated list
        /// </summary>
        /// <param name="pageNumber">The current page number (1-based)</param>
        /// <param name="pageSize">The number of items per page</param>
        /// <returns>An empty paginated list</returns>
        public static PaginatedList<T> Empty(int pageNumber = 1, int pageSize = 10)
        {
            return new PaginatedList<T>(Enumerable.Empty<T>(), 0, pageNumber, pageSize);
        }

        /// <summary>
        /// Creates a paginated list from an IQueryable source
        /// </summary>
        /// <param name="source">The source queryable</param>
        /// <param name="pageNumber">The current page number (1-based)</param>
        /// <param name="pageSize">The number of items per page</param>
        /// <returns>A paginated list containing the items</returns>
        public static async Task<PaginatedList<T>> CreateAsync(
            IQueryable<T> source, 
            int pageNumber, 
            int pageSize)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
                
            var count = await source.CountAsync();
            var items = await source
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
                
            return new PaginatedList<T>(items, count, pageNumber, pageSize);
        }

        /// <summary>
        /// Projects the elements of the paginated list to a new form
        /// </summary>
        /// <typeparam name="TResult">The type of the result elements</typeparam>
        /// <param name="selector">A transform function to apply to each element</param>
        /// <returns>A new paginated list containing the projected elements</returns>
        public PaginatedList<TResult> Select<TResult>(Func<T, TResult> selector)
        {
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));
                
            var projectedItems = _items.Select(selector).ToList();
            return new PaginatedList<TResult>(
                projectedItems, 
                TotalCount, 
                PageNumber, 
                PageSize);
        }
    }
}
