
namespace FCG.Payments.Infrastructure.Kafka.Consumers.OrderPlaced
{
    public record OrderPlacedEvent(string UserEmail, Guid CorrelationId, Guid UserId, Guid GameId, decimal Amount, DateTime CreatedAt);
}
