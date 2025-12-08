namespace FCG.Payments.Infrastructure.Kafka.Consumers.UserCreated
{
    public record UserCreatedEvent(string UserId, string Name, string Email, string CorrelationId, string CreatedAt);
}
