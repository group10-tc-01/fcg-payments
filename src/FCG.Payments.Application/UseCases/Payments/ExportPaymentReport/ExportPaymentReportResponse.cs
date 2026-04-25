namespace FCG.Payments.Application.UseCases.Payments.ExportPaymentReport
{
    public sealed record ExportPaymentReportResponse(
        byte[] Content,
        string ContentType,
        string FileName);
}
