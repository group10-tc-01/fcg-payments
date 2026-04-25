namespace FCG.Payments.Application.UseCases.Payments.ExportPaymentReport
{
    public sealed class PaymentReportSettings
    {
        public const string SectionName = "PaymentReports";
        public const int DefaultPdfExportMaxRecords = 100;

        public int PdfExportMaxRecords { get; set; } = DefaultPdfExportMaxRecords;

        public int GetEffectivePdfExportMaxRecords()
        {
            return PdfExportMaxRecords > 0 ? PdfExportMaxRecords : DefaultPdfExportMaxRecords;
        }
    }
}
