using FCG.Payments.Application.UseCases.Payments.ProcessPayment;
using FCG.Payments.Infrastructure.Kafka.Abstractions;
using FCG.Payments.Infrastructure.Kafka.Settings;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FCG.Payments.Infrastructure.Kafka.Consumers.OrderPlaced
{
    public sealed class OrderPlacedConsumer : BaseKafkaConsumer<OrderPlacedEvent>
    {
        private readonly ILogger<OrderPlacedConsumer> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public OrderPlacedConsumer(ILogger<OrderPlacedConsumer> logger, IOptions<KafkaSettings> kafkaSettings, IServiceScopeFactory serviceScopeFactory)
            : base(
                logger,
                kafkaSettings.Value.BootstrapServers,
                kafkaSettings.Value.GroupId,
                kafkaSettings.Value.Topics.OrderPlaced,
                kafkaSettings.Value.UseSaslSsl,
                kafkaSettings.Value.SaslUsername,
                kafkaSettings.Value.SaslPassword,
                kafkaSettings.Value.ConsumerTimeoutMs)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task ProcessEventAsync(OrderPlacedEvent @event, CancellationToken cancellationToken)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            try
            {
                _logger.LogInformation("Processing OrderPlacedEvent. CorrelationId: {CorrelationId}, UserId: {UserId}, GameId: {GameId}, Amount: {Amount}", @event.CorrelationId, @event.UserId, @event.GameId, @event.Amount);

                var request = new ProcessPaymentRequest(@event.UserEmail, @event.CorrelationId, @event.UserId, @event.GameId, @event.Amount);

                await mediator.Send(request, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing OrderPlacedEvent. CorrelationId: {CorrelationId}", @event.CorrelationId);
                throw;
            }
        }
    }
}
