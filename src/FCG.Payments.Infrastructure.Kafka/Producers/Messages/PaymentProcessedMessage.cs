namespace FCG.Payments.Infrastructure.Kafka.Producers.Messages
{
    public record PaymentProcessedMessage(Guid CorrelationId, Guid PaymentId, Guid UserId, Guid GameId, decimal Amount, string Status, DateTime ProcessedAt);
}
