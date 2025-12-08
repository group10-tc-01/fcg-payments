using FCG.Payments.Domain.Payments;

namespace FCG.Payments.Application.UseCases.Payments.GetPaymentHistory
{
    public record GetPaymentHistoryResponse(
        Guid Id,
        Guid UserId,
        Guid GameId,
        Guid WalletId,
        decimal Amount,
        PaymentStatus Status,
        string? FailureReason,
        DateTime? ProcessedAt,
        DateTime CreatedAt);
}
