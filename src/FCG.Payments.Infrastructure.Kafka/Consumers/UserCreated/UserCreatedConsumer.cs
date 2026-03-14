using FCG.Payments.Application.UseCases.Wallets.CreateWallet;
using FCG.Payments.Infrastructure.Kafka.Abstractions;
using FCG.Payments.Infrastructure.Kafka.Settings;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FCG.Payments.Infrastructure.Kafka.Consumers.UserCreated
{
    public sealed class UserCreatedConsumer : BaseKafkaConsumer<UserCreatedEvent>
    {
        private readonly ILogger<UserCreatedConsumer> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public UserCreatedConsumer(ILogger<UserCreatedConsumer> logger, IOptions<KafkaSettings> kafkaSettings, IServiceScopeFactory serviceScopeFactory)
            : base(
                logger,
                kafkaSettings.Value.BootstrapServers,
                kafkaSettings.Value.GroupId,
                kafkaSettings.Value.Topics.UserCreated,
                kafkaSettings.Value.UseSaslSsl,
                kafkaSettings.Value.SaslUsername,
                kafkaSettings.Value.SaslPassword,
                kafkaSettings.Value.ConsumerTimeoutMs)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task ProcessEventAsync(UserCreatedEvent @event, CancellationToken cancellationToken)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            try
            {
                _logger.LogInformation("Processing UserCreatedEvent for UserId: {UserId}", @event.UserId);

                var createWalletRequest = new CreateWalletRequest(new Guid(@event.UserId));

                await mediator.Send(createWalletRequest, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing UserCreatedEvent for UserId: {UserId}", @event.UserId);
                throw;
            }
        }
    }
}
