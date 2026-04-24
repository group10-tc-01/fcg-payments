namespace FCG.Payments.Domain.Payments.Reports
{
    public sealed record PaymentReportSummary(
        int TotalPayments,
        int TotalApproved,
        int TotalRejected,
        decimal ApprovedAmount,
        decimal RejectedAmount);
}
