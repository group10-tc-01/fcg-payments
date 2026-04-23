namespace FCG.Payments.Domain.Payments.Reports
{
    public sealed record PaymentReport(
        Guid PaymentId,
        Guid UserId,
        Guid GameId,
        decimal Amount,
        PaymentStatus Status,
        DateTime ProcessedAt);
}
