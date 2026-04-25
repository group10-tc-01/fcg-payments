namespace FCG.Payments.Domain.Payments.Reports
{
    public sealed record PaymentReportFilter(
        PaymentStatus? Status,
        DateTime? DateFrom,
        DateTime? DateTo,
        Guid? UserId,
        Guid? GameId)
    {
        public static PaymentReportFilter Empty { get; } = new(null, null, null, null, null);
    }
}
