using FCG.Payments.Application.Abstractions.Messaging;

namespace FCG.Payments.Application.UseCases.Payments.GetPaymentReport
{
    public record GetPaymentReportRequest(
        int PageNumber,
        int PageSize,
        PaymentReportStatusFilter? Status,
        DateTime? DateFrom,
        DateTime? DateTo,
        Guid? UserId,
        Guid? GameId) : IQuery<GetPaymentReportResponse>;
}
