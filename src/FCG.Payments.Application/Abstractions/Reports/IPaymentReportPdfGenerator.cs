using FCG.Payments.Application.UseCases.Payments.ExportPaymentReport;

namespace FCG.Payments.Application.Abstractions.Reports
{
    public interface IPaymentReportPdfGenerator
    {
        byte[] Generate(PaymentReportPdfData report);
    }
}
