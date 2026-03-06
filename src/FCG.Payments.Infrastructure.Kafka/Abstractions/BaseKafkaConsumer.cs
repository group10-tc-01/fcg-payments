using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace FCG.Payments.Infrastructure.Kafka.Abstractions
{
    [ExcludeFromCodeCoverage]
    public abstract class BaseKafkaConsumer<TEvent> : BackgroundService, IKafkaConsumer where TEvent : class
    {
        private readonly ILogger<BaseKafkaConsumer<TEvent>> _logger;
        private readonly IConsumer<string, string> _consumer;
        private readonly string _topic;
        private readonly int _consumerTimeoutMs;

        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        protected BaseKafkaConsumer(
            ILogger<BaseKafkaConsumer<TEvent>> logger,
            string bootstrapServers,
            string groupId,
            string topic,
            bool useSaslSsl,
            string saslUsername,
            string saslPassword,
            int consumerTimeoutMs = 100)
        {
            _logger = logger;
            _topic = topic;
            _consumerTimeoutMs = consumerTimeoutMs;

            var config = new ConsumerConfig
            {
                BootstrapServers = bootstrapServers,
                GroupId = groupId,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            };

            if (useSaslSsl)
            {
                config.SecurityProtocol = SecurityProtocol.SaslSsl;
                config.SaslMechanism = SaslMechanism.Plain;
                config.SaslUsername = saslUsername;
                config.SaslPassword = saslPassword;
            }

            _consumer = new ConsumerBuilder<string, string>(config).Build();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _consumer.Subscribe(_topic);

            _logger.LogInformation("Kafka consumer started for topic: {Topic}", _topic);

            try
            {
                await Task.Yield();
                await ConsumeAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Kafka consumer for topic: {Topic}", _topic);
            }
            finally
            {
                _consumer.Close();
            }
        }

        public async Task ConsumeAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = _consumer.Consume(TimeSpan.FromMilliseconds(_consumerTimeoutMs));

                    if (consumeResult?.Message?.Value != null)
                    {
                        _logger.LogInformation(
                            "Message received from topic {Topic}, partition {Partition}, offset {Offset}, event type {EventType}",
                            _topic,
                            consumeResult.Partition.Value,
                            consumeResult.Offset.Value,
                            typeof(TEvent).Name);

                        var @event = JsonSerializer.Deserialize<TEvent>(consumeResult.Message.Value, _jsonOptions);

                        if (@event != null)
                        {
                            await ProcessEventAsync(@event, cancellationToken);

                            _consumer.Commit(consumeResult);

                            _logger.LogInformation("Message processed and committed successfully from topic {Topic}", _topic);
                        }
                    }

                    await Task.Delay(1, cancellationToken);
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Error consuming message from topic {Topic}", _topic);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Kafka consumer for topic {Topic} is shutting down", _topic);
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message from topic {Topic}", _topic);
                }
            }
        }

        protected abstract Task ProcessEventAsync(TEvent @event, CancellationToken cancellationToken);

        public override void Dispose()
        {
            _consumer?.Dispose();
            base.Dispose();
        }
    }
}
