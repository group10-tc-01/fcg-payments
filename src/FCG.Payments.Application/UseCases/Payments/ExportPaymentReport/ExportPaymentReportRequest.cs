using FCG.Payments.Application.Abstractions.Messaging;

namespace FCG.Payments.Application.UseCases.Payments.ExportPaymentReport
{
    public sealed record ExportPaymentReportRequest(
        PaymentReportStatusFilter? Status,
        DateTime? DateFrom,
        DateTime? DateTo,
        Guid? UserId,
        Guid? GameId) : IQuery<ExportPaymentReportResponse>;
}
