using FCG.Payments.Application.Abstractions.Messaging;

namespace FCG.Payments.Application.UseCases.Payments.GetPaymentReport
{
    public record GetPaymentReportRequest(int PageNumber, int PageSize) : IQuery<GetPaymentReportResponse>;
}
