using FCG.Payments.Application.Abstractions.Messaging;

namespace FCG.Payments.Application.UseCases.Payments.GetPaymentReport
{
    public interface IGetPaymentReportUseCase : IQueryHandler<GetPaymentReportRequest, GetPaymentReportResponse> { }
}
