namespace FCG.Payments.Infrastructure.Kafka.Consumers.OrderPlaced
{
    public record OrderPlacedEvent(string CorrelationId, string UserId, string GameId, decimal Amount, string CreatedAt);
}
