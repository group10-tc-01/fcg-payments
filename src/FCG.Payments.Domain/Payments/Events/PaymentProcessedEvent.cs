using FCG.Payments.Domain.Abstractions;

namespace FCG.Payments.Domain.Payments.Events
{
    public record PaymentProcessedEvent(Guid PaymentId, Guid UserId, Guid GameId, decimal Amount, PaymentStatus Status, DateTime ProcessedAt) : IDomainEvent;
}
