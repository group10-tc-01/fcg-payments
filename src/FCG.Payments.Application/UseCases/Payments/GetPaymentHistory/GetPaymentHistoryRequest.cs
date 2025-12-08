using FCG.Payments.Application.Abstractions.Messaging;
using FCG.Payments.Application.Abstractions.Pagination;
using FCG.Payments.Domain.Payments;

namespace FCG.Payments.Application.UseCases.Payments.GetPaymentHistory
{
    public record GetPaymentHistoryRequest(
        int PageNumber,
        int PageSize,
        PaymentStatus? Status,
        DateTime? DateFrom,
        DateTime? DateTo) : ICommand<PagedListResponse<GetPaymentHistoryResponse>>;
}
