using System.Diagnostics.CodeAnalysis;

namespace FCG.Payments.Infrastructure.Kafka.Producers.Messages
{
    [ExcludeFromCodeCoverage]
    public record PaymentProcessedMessage(string UserEmail, Guid CorrelationId, Guid PaymentId, Guid UserId, Guid GameId, decimal Amount, string Status, DateTime ProcessedAt);
}
