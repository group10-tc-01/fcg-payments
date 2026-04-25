using FCG.Payments.Application.UseCases.Payments.GetPaymentReport;
using FCG.Payments.Domain.Payments.Reports;

namespace FCG.Payments.Application.UseCases.Payments.ExportPaymentReport
{
    public sealed record PaymentReportPdfData(
        IReadOnlyList<GetPaymentReportItemResponse> Items,
        GetPaymentReportSummaryResponse Summary,
        PaymentReportFilter Filter,
        DateTime GeneratedAt,
        int TotalMatched);
}
