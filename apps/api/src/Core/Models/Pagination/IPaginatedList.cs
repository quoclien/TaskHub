using System.Collections.Generic;

namespace TaskHub.Api.Core.Models.Pagination
{
    /// <summary>
    /// Represents a paginated list of items with pagination metadata
    /// </summary>
    /// <typeparam name="T">The type of items in the list</typeparam>
    public interface IPaginatedList<out T>
    {
        /// <summary>
        /// Gets the current page number (1-based)
        /// </summary>
        int PageNumber { get; }

        /// <summary>
        /// Gets the number of items per page
        /// </summary>
        int PageSize { get; }

        /// <summary>
        /// Gets the total number of items across all pages
        /// </summary>
        int TotalCount { get; }

        /// <summary>
        /// Gets the total number of pages
        /// </summary>
        int TotalPages { get; }

        /// <summary>
        /// Gets a value indicating whether there is a previous page
        /// </summary>
        bool HasPreviousPage { get; }

        /// <summary>
        /// Gets a value indicating whether there is a next page
        /// </summary>
        bool HasNextPage { get; }

        /// <summary>
        /// Gets the collection of items for the current page
        /// </summary>
        IReadOnlyCollection<T> Items { get; }
    }
}
