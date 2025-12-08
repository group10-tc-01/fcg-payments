namespace FCG.Payments.Infrastructure.Kafka.Abstractions
{
    public interface IKafkaConsumer
    {
        Task ConsumeAsync(CancellationToken cancellationToken);
    }
}
