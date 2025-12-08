using Confluent.Kafka;
using FCG.Payments.Application.Abstractions.Messaging;
using FCG.Payments.Infrastructure.Kafka.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace FCG.Payments.Infrastructure.Kafka.Producers
{
    [ExcludeFromCodeCoverage]
    public sealed class KafkaMessageProducer : IMessageProducer, IDisposable
    {
        private readonly IProducer<string, string> _producer;
        private readonly ILogger<KafkaMessageProducer> _logger;
        private bool _disposed;

        public KafkaMessageProducer(IOptions<KafkaSettings> kafkaSettings, ILogger<KafkaMessageProducer> logger)
        {
            _logger = logger;

            var config = new ProducerConfig
            {
                BootstrapServers = kafkaSettings.Value.BootstrapServers,
                Acks = Acks.All,
                EnableIdempotence = true,
                MaxInFlight = 5,
                MessageSendMaxRetries = 3
            };

            _producer = new ProducerBuilder<string, string>(config).Build();
        }

        public async Task ProduceAsync<T>(string topic, T message, CancellationToken cancellationToken = default) where T : class
        {
            try
            {
                var serializedMessage = JsonSerializer.Serialize(message);
                var kafkaMessage = new Message<string, string>
                {
                    Key = Guid.NewGuid().ToString(),
                    Value = serializedMessage
                };

                _logger.LogInformation("Producing message {Message} to topic {Topic}", serializedMessage, topic);

                var deliveryResult = await _producer.ProduceAsync(topic, kafkaMessage, cancellationToken);

                _logger.LogInformation(
                    "Message delivered to topic {Topic}, partition {Partition}, offset {Offset}",
                    deliveryResult.Topic,
                    deliveryResult.Partition.Value,
                    deliveryResult.Offset.Value);
            }
            catch (ProduceException<string, string> ex)
            {
                _logger.LogError(ex, "Failed to produce message to topic {Topic}. Error: {ErrorReason}", topic, ex.Error.Reason);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while producing message to topic {Topic}", topic);
                throw;
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _producer?.Flush(TimeSpan.FromSeconds(10));
            _producer?.Dispose();
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
