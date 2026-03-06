using Confluent.Kafka;
using FCG.Payments.Application.Abstractions.Messaging;
using FCG.Payments.Infrastructure.Kafka.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;

namespace FCG.Payments.Infrastructure.Kafka.Producers
{
    [ExcludeFromCodeCoverage]
    public sealed class KafkaMessageProducer : IMessageProducer, IDisposable
    {
        private static readonly ActivitySource ActivitySource = new("FCG.Payments");
        private const string TraceParentHeaderName = "traceparent";
        private const string TraceStateHeaderName = "tracestate";
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

            if (kafkaSettings.Value.UseSaslSsl)
            {
                config.SecurityProtocol = SecurityProtocol.SaslSsl;
                config.SaslMechanism = SaslMechanism.Plain;
                config.SaslUsername = kafkaSettings.Value.SaslUsername;
                config.SaslPassword = kafkaSettings.Value.SaslPassword;
            }

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
                    Value = serializedMessage,
                    Headers = new Headers()
                };

                using Activity? activity = ActivitySource.StartActivity("kafka publish", ActivityKind.Producer);

                activity?.SetTag("messaging.system", "kafka");
                activity?.SetTag("messaging.destination.name", topic);
                activity?.SetTag("messaging.operation", "publish");
                activity?.SetTag("messaging.message.id", kafkaMessage.Key);

                AddTraceHeaders(kafkaMessage.Headers, activity);

                _logger.LogInformation(
                    "Producing message to topic {Topic} with key {MessageKey} and type {MessageType}",
                    topic,
                    kafkaMessage.Key,
                    typeof(T).Name);

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

        private static void AddTraceHeaders(Headers headers, Activity? activity)
        {
            if (activity is null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(activity.Id))
            {
                return;
            }

            headers.Remove(TraceParentHeaderName);
            headers.Remove(TraceStateHeaderName);

            headers.Add(TraceParentHeaderName, Encoding.UTF8.GetBytes(activity.Id));

            if (!string.IsNullOrWhiteSpace(activity.TraceStateString))
            {
                headers.Add(TraceStateHeaderName, Encoding.UTF8.GetBytes(activity.TraceStateString));
            }
        }
    }
}
