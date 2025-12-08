using FCG.Payments.Infrastructure.Kafka.Abstractions;
using FCG.Payments.Infrastructure.Kafka.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FCG.Payments.Infrastructure.Kafka.Consumers.UserCreated
{
    public sealed class UserCreatedConsumer : BaseKafkaConsumer<UserCreatedEvent>
    {
        private readonly ILogger<UserCreatedConsumer> _logger;

        public UserCreatedConsumer(ILogger<UserCreatedConsumer> logger, IOptions<KafkaSettings> kafkaSettings)
            : base(logger, kafkaSettings.Value.BootstrapServers, kafkaSettings.Value.GroupId, kafkaSettings.Value.Topics.UserCreated, kafkaSettings.Value.ConsumerTimeoutMs)
        {
            _logger = logger;
        }

        protected override async Task ProcessEventAsync(UserCreatedEvent @event, CancellationToken cancellationToken)
        {
            try
            {
                await Task.Delay(100, cancellationToken);

                _logger.LogInformation("Processing UserCreatedEvent for UserId: {UserId}", @event.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing UserCreatedEvent for UserId: {UserId}", @event.UserId);
                throw;
            }
        }
    }
}
