using FCG.Payments.Infrastructure.Kafka.Consumers.OrderPlaced;
using FCG.Payments.Infrastructure.Kafka.Settings;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace FCG.Payments.UnitTests.Infrastructure.Kafka.Consumers.OrderPlaced
{
    public class OrderPlacedConsumerTest
    {
        private readonly Mock<ILogger<OrderPlacedConsumer>> _loggerMock;
        private readonly IOptions<KafkaSettings> _kafkaSettings;
        private readonly OrderPlacedConsumer _sut;

        public OrderPlacedConsumerTest()
        {
            _loggerMock = new Mock<ILogger<OrderPlacedConsumer>>();

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

            _sut = new OrderPlacedConsumer(_loggerMock.Object, _kafkaSettings);
        }

        [Fact]
        public async Task ProcessEventAsync_WithValidEvent_ShouldLogInformation()
        {
            // Arrange
            var correlationId = Guid.NewGuid().ToString();
            var userId = Guid.NewGuid().ToString();
            var gameId = Guid.NewGuid().ToString();
            var amount = 99.99m;
            var createdAt = DateTime.UtcNow.ToString("O");

            var @event = new OrderPlacedEvent(correlationId, userId, gameId, amount, createdAt);

            // Act
            await InvokeProcessEventAsync(@event, CancellationToken.None);

            // Assert
            _loggerMock
                .Invocations
                .Should().ContainSingle(inv => 
                    inv.Method.Name == "Log" &&
                    inv.Arguments[0].Equals(LogLevel.Information) &&
                    inv.Arguments[2].ToString()!.Contains("Processing OrderPlacedEvent") &&
                    inv.Arguments[2].ToString()!.Contains(correlationId) &&
                    inv.Arguments[2].ToString()!.Contains(userId) &&
                    inv.Arguments[2].ToString()!.Contains(gameId) &&
                    inv.Arguments[2].ToString()!.Contains(amount.ToString())
                );
        }

        [Fact]
        public async Task ProcessEventAsync_WithValidEvent_ShouldCompleteSuccessfully()
        {
            // Arrange
            var @event = new OrderPlacedEvent(
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                150.00m,
                DateTime.UtcNow.ToString("O")
            );

            // Act
            var act = async () => await InvokeProcessEventAsync(@event, CancellationToken.None);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task ProcessEventAsync_WhenCancellationRequested_ShouldThrowOperationCanceledException()
        {
            // Arrange
            var @event = new OrderPlacedEvent(
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                200.00m,
                DateTime.UtcNow.ToString("O")
            );

            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act
            var act = async () => await InvokeProcessEventAsync(@event, cts.Token);

            // Assert
            await act.Should().ThrowAsync<OperationCanceledException>();
        }

        [Fact]
        public async Task ProcessEventAsync_WhenExceptionOccursInDelay_ShouldLogErrorAndRethrow()
        {
            // Arrange
            var correlationId = Guid.NewGuid().ToString();
            var @event = new OrderPlacedEvent(
                correlationId,
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                100.00m,
                DateTime.UtcNow.ToString("O")
            );

            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act
            var act = async () => await InvokeProcessEventAsync(@event, cts.Token);

            // Assert
            await act.Should().ThrowAsync<OperationCanceledException>();

            _loggerMock
                .Invocations
                .Should().ContainSingle(inv => 
                    inv.Method.Name == "Log" &&
                    inv.Arguments[0].Equals(LogLevel.Error) &&
                    inv.Arguments[2].ToString()!.Contains("Error processing OrderPlacedEvent") &&
                    inv.Arguments[2].ToString()!.Contains(correlationId)
                );
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
