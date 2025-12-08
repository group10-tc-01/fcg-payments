using FCG.Payments.Infrastructure.Kafka.Abstractions;
using FCG.Payments.Infrastructure.Kafka.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FCG.Payments.Infrastructure.Kafka.Consumers.OrderPlaced
{
    public sealed class OrderPlacedConsumer : BaseKafkaConsumer<OrderPlacedEvent>
    {
        private readonly ILogger<OrderPlacedConsumer> _logger;

        public OrderPlacedConsumer(ILogger<OrderPlacedConsumer> logger, IOptions<KafkaSettings> kafkaSettings)
            : base(logger, kafkaSettings.Value.BootstrapServers, kafkaSettings.Value.GroupId, kafkaSettings.Value.Topics.OrderPlaced, kafkaSettings.Value.ConsumerTimeoutMs)
        {
            _logger = logger;
        }

        protected override async Task ProcessEventAsync(OrderPlacedEvent @event, CancellationToken cancellationToken)
        {
            try
            {
                await Task.Delay(100, cancellationToken);

                _logger.LogInformation("Processing OrderPlacedEvent. CorrelationId: {CorrelationId}, UserId: {UserId}, GameId: {GameId}, Amount: {Amount}", @event.CorrelationId, @event.UserId, @event.GameId, @event.Amount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing OrderPlacedEvent. CorrelationId: {CorrelationId}", @event.CorrelationId);
                throw;
            }
        }
    }
}
