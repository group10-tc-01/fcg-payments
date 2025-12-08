namespace FCG.Payments.Infrastructure.Kafka.Consumers.OrderPlaced
{
    public record OrderPlacedEvent(Guid CorrelationId, Guid UserId, Guid GameId, decimal Amount, DateTime CreatedAt);
}
