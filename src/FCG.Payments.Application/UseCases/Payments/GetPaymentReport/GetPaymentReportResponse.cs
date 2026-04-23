using FCG.Payments.Domain.Payments;
using System.Text.Json.Serialization;

namespace FCG.Payments.Application.UseCases.Payments.GetPaymentReport
{
    public sealed record GetPaymentReportResponse
    {
        public IReadOnlyList<GetPaymentReportItemResponse> Items { get; init; }
        public int CurrentPage { get; init; }
        public int TotalPages { get; init; }
        public int PageSize { get; init; }
        public int TotalCount { get; init; }
        public bool HasPrevious => CurrentPage > 1;
        public bool HasNext => CurrentPage < TotalPages;
        public GetPaymentReportSummaryResponse Summary { get; init; }

        [JsonConstructor]
        public GetPaymentReportResponse(
            IReadOnlyList<GetPaymentReportItemResponse> items,
            int currentPage,
            int totalPages,
            int pageSize,
            int totalCount,
            GetPaymentReportSummaryResponse summary)
        {
            Items = items;
            CurrentPage = currentPage;
            TotalPages = totalPages;
            PageSize = pageSize;
            TotalCount = totalCount;
            Summary = summary;
        }

        public GetPaymentReportResponse(
            IEnumerable<GetPaymentReportItemResponse> items,
            int totalCount,
            int currentPage,
            int pageSize,
            GetPaymentReportSummaryResponse summary)
        {
            Items = items.ToList().AsReadOnly();
            TotalCount = totalCount;
            PageSize = pageSize;
            CurrentPage = currentPage;
            TotalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);
            Summary = summary;
        }
    }

    public sealed record GetPaymentReportItemResponse(
        Guid PaymentId,
        Guid UserId,
        Guid GameId,
        decimal Amount,
        PaymentStatus Status,
        DateTime ProcessedAt);

    public sealed record GetPaymentReportSummaryResponse(
        int TotalPayments,
        int TotalApproved,
        int TotalRejected,
        decimal ApprovedAmount,
        decimal RejectedAmount);
}
