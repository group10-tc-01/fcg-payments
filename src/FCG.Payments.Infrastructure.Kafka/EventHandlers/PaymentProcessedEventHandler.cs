using FCG.Payments.Application.Abstractions.Messaging;
using FCG.Payments.Domain.Payments.Events;
using FCG.Payments.Infrastructure.Kafka.Producers.Messages;
using FCG.Payments.Infrastructure.Kafka.Settings;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;

namespace FCG.Payments.Infrastructure.Kafka.EventHandlers
{
    [ExcludeFromCodeCoverage]
    public class PaymentProcessedEventHandler : INotificationHandler<PaymentProcessedEvent>
    {
        private readonly IMessageProducer _messageProducer;
        private readonly IOptions<KafkaSettings> _kafkaSettings;
        private readonly ILogger<PaymentProcessedEventHandler> _logger;

        public PaymentProcessedEventHandler(
            IMessageProducer messageProducer,
            IOptions<KafkaSettings> kafkaSettings,
            ILogger<PaymentProcessedEventHandler> logger)
        {
            _messageProducer = messageProducer;
            _kafkaSettings = kafkaSettings;
            _logger = logger;
        }

        public async Task Handle(PaymentProcessedEvent domainEvent, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Processing PaymentProcessedEvent for UserId {UserId}", domainEvent.UserId);

            var message = new PaymentProcessedMessage(
                CorrelationId: Guid.NewGuid(),
                PaymentId: domainEvent.PaymentId,
                UserId: domainEvent.UserId,
                GameId: domainEvent.GameId,
                Amount: domainEvent.Amount,
                Status: domainEvent.Status.ToString(),
                ProcessedAt: DateTime.UtcNow
            );

            _logger.LogInformation(
                "Created PaymentProcessedMessage for UserId {UserId}, Topic {Topic}",
                message.UserId, _kafkaSettings.Value.Topics.PaymentProcessed);

            try
            {
                await _messageProducer.ProduceAsync(_kafkaSettings.Value.Topics.PaymentProcessed, message, cancellationToken);

                _logger.LogInformation("Successfully completed PaymentProcessedEvent processing for UserId {UserId}", domainEvent.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process PaymentProcessedEvent for UserId {UserId}", domainEvent.UserId);

                throw;
            }
        }
    }
}
