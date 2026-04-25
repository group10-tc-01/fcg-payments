namespace FCG.Payments.Application.UseCases.Payments.ExportPaymentReport
{
    public interface IExportPaymentReportUseCase : FCG.Payments.Application.Abstractions.Messaging.IQueryHandler<ExportPaymentReportRequest, ExportPaymentReportResponse> { }
}
