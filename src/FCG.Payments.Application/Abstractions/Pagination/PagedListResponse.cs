using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace FCG.Payments.Application.Abstractions.Pagination
{
    [ExcludeFromCodeCoverage]
    public sealed record PagedListResponse<T>
    {
        public IReadOnlyList<T> Items { get; init; }
        public int CurrentPage { get; init; }
        public int TotalPages { get; init; }
        public int PageSize { get; init; }
        public int TotalCount { get; init; }
        public bool HasPrevious => CurrentPage > 1;
        public bool HasNext => CurrentPage < TotalPages;

        [JsonConstructor]
        public PagedListResponse(IReadOnlyList<T> items, int currentPage, int totalPages, int pageSize, int totalCount)
        {
            Items = items;
            CurrentPage = currentPage;
            TotalPages = totalPages;
            PageSize = pageSize;
            TotalCount = totalCount;
        }

        public PagedListResponse(IEnumerable<T> items, int totalCount, int currentPage, int pageSize)
        {
            Items = items.ToList().AsReadOnly();
            TotalCount = totalCount;
            PageSize = pageSize;
            CurrentPage = currentPage;
            TotalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);
        }
    }
}
