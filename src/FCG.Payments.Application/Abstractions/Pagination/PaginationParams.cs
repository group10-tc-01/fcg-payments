namespace FCG.Payments.Application.Abstractions.Pagination
{
    public sealed record PaginationParams
    {
        private const int MinPageNumber = 1;
        private const int MinPageSize = 1;
        private const int MaxPageSize = 50;

        public int PageNumber { get; init; }
        public int PageSize { get; init; }

        public PaginationParams(int pageNumber, int pageSize)
        {
            ValidatePageNumber(pageNumber);
            ValidatePageSize(pageSize);

            PageNumber = pageNumber;
            PageSize = pageSize;
        }

        private static void ValidatePageNumber(int pageNumber)
        {
            if (pageNumber < MinPageNumber)
            {
                throw new ArgumentException(
                    $"PageNumber must be greater than or equal to {MinPageNumber}.",
                    nameof(pageNumber));
            }
        }

        private static void ValidatePageSize(int pageSize)
        {
            if (pageSize < MinPageSize || pageSize > MaxPageSize)
            {
                throw new ArgumentException(
                    $"PageSize must be between {MinPageSize} and {MaxPageSize}.",
                    nameof(pageSize));
            }
        }
    }
}
