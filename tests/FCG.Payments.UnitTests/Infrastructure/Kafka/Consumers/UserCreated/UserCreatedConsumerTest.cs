using FCG.Payments.Application.UseCases.Wallets.CreateWallet;
using FCG.Payments.Infrastructure.Kafka.Consumers.UserCreated;
using FCG.Payments.Infrastructure.Kafka.Settings;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace FCG.Payments.UnitTests.Infrastructure.Kafka.Consumers.UserCreated
{
    public class UserCreatedConsumerTest
    {
        private readonly Mock<ILogger<UserCreatedConsumer>> _loggerMock;
        private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
        private readonly Mock<IServiceScope> _serviceScopeMock;
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly Mock<IMediator> _mediatorMock;
        private readonly IOptions<KafkaSettings> _kafkaSettings;
        private readonly UserCreatedConsumer _sut;

        public UserCreatedConsumerTest()
        {
            _loggerMock = new Mock<ILogger<UserCreatedConsumer>>();
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

            _sut = new UserCreatedConsumer(_loggerMock.Object, _kafkaSettings, _serviceScopeFactoryMock.Object);
        }

        [Fact]
        public async Task ProcessEventAsync_WithValidEvent_ShouldSendCreateWalletRequest()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var @event = new UserCreatedEvent(
                userId.ToString(),
                "Test User",
                "test@example.com",
                Guid.NewGuid().ToString(),
                DateTime.UtcNow.ToString("O")
            );

            _mediatorMock
                .Setup(x => x.Send(It.IsAny<CreateWalletRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CreateWalletResponse(Guid.NewGuid()));

            // Act
            await InvokeProcessEventAsync(@event, CancellationToken.None);

            // Assert
            _mediatorMock
                .Invocations
                .Where(inv => inv.Method.Name == nameof(IMediator.Send))
                .Should().ContainSingle()
                .Which.Arguments[0].Should().BeOfType<CreateWalletRequest>()
                .Which.UserId.Should().Be(userId);
        }

        [Fact]
        public async Task ProcessEventAsync_WithValidEvent_ShouldLogInformation()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var @event = new UserCreatedEvent(
                userId.ToString(),
                "Test User",
                "test@example.com",
                Guid.NewGuid().ToString(),
                DateTime.UtcNow.ToString("O")
            );

            _mediatorMock
                .Setup(x => x.Send(It.IsAny<CreateWalletRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CreateWalletResponse(Guid.NewGuid()));

            // Act
            await InvokeProcessEventAsync(@event, CancellationToken.None);

            // Assert
            _loggerMock
                .Invocations
                .Should().ContainSingle(inv => 
                    inv.Method.Name == "Log" &&
                    inv.Arguments[0].Equals(LogLevel.Information) &&
                    inv.Arguments[2].ToString()!.Contains($"Processing UserCreatedEvent for UserId: {userId}")
                );
        }

        [Fact]
        public async Task ProcessEventAsync_WhenMediatorThrowsException_ShouldLogErrorAndRethrow()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var @event = new UserCreatedEvent(
                userId.ToString(),
                "Test User",
                "test@example.com",
                Guid.NewGuid().ToString(),
                DateTime.UtcNow.ToString("O")
            );

            var expectedException = new InvalidOperationException("Mediator error");

            _mediatorMock
                .Setup(x => x.Send(It.IsAny<CreateWalletRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(expectedException);

            // Act
            var act = async () => await InvokeProcessEventAsync(@event, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Mediator error");

            _loggerMock
                .Invocations
                .Should().ContainSingle(inv => 
                    inv.Method.Name == "Log" &&
                    inv.Arguments[0].Equals(LogLevel.Error) &&
                    inv.Arguments[2].ToString()!.Contains($"Error processing UserCreatedEvent for UserId: {userId}") &&
                    inv.Arguments[3] == expectedException
                );
        }

        private async Task InvokeProcessEventAsync(UserCreatedEvent @event, CancellationToken cancellationToken)
        {
            var method = typeof(UserCreatedConsumer).GetMethod(
                "ProcessEventAsync",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
            );

            var task = (Task)method!.Invoke(_sut, new object[] { @event, cancellationToken })!;
            await task;
        }
    }
}
