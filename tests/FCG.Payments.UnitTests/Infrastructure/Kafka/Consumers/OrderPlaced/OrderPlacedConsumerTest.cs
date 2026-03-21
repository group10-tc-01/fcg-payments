using FCG.Payments.Infrastructure.Kafka.Consumers.OrderPlaced;
using FCG.Payments.Infrastructure.Kafka.Settings;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace FCG.Payments.UnitTests.Infrastructure.Kafka.Consumers.OrderPlaced
{
    public class OrderPlacedConsumerTest
    {
        private readonly Mock<ILogger<OrderPlacedConsumer>> _loggerMock;
        private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
        private readonly Mock<IServiceScope> _serviceScopeMock;
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly Mock<IMediator> _mediatorMock;
        private readonly IOptions<KafkaSettings> _kafkaSettings;
        private readonly OrderPlacedConsumer _sut;

        public OrderPlacedConsumerTest()
        {
            _loggerMock = new Mock<ILogger<OrderPlacedConsumer>>();
            _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            _serviceScopeMock = new Mock<IServiceScope>();
            _serviceProviderMock = new Mock<IServiceProvider>();
            _mediatorMock = new Mock<IMediator>();

            _kafkaSettings = Options.Create(new KafkaSettings
            {
                BootstrapServers = "localhost:9092",
                GroupId = "test-group",
                ConsumerTimeoutMs = 100,
                Topics = new KafkaTopics
                {
                    UserCreated = "user-created-topic",
                    OrderPlaced = "order-placed-topic"
                }
            });

            _serviceScopeFactoryMock.Setup(x => x.CreateScope()).Returns(_serviceScopeMock.Object);
            _serviceScopeMock.Setup(x => x.ServiceProvider).Returns(_serviceProviderMock.Object);
            _serviceProviderMock.Setup(x => x.GetService(typeof(IMediator))).Returns(_mediatorMock.Object);

            _sut = new OrderPlacedConsumer(_loggerMock.Object, _kafkaSettings, _serviceScopeFactoryMock.Object);
        }

        [Fact]
        public async Task ProcessEventAsync_WithValidEvent_ShouldCompleteSuccessfully()
        {
            // Arrange
            var @event = new OrderPlacedEvent(
                "teste@gmail.com",
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                150.00m,
                DateTime.UtcNow
            );

            // Act
            var act = async () => await InvokeProcessEventAsync(@event, CancellationToken.None);

            // Assert
            await act.Should().NotThrowAsync();
        }

        private async Task InvokeProcessEventAsync(OrderPlacedEvent @event, CancellationToken cancellationToken)
        {
            var method = typeof(OrderPlacedConsumer).GetMethod(
                "ProcessEventAsync",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
            );

            var task = (Task)method!.Invoke(_sut, new object[] { @event, cancellationToken })!;
            await task;
        }
    }
}
