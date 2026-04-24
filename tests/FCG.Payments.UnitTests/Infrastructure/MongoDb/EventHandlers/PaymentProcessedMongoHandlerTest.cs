using FCG.Payments.Domain.Payments;
using FCG.Payments.Domain.Payments.Events;
using FCG.Payments.Domain.Payments.Reports;
using FCG.Payments.Infrastructure.MongoDb.EventHandlers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace FCG.Payments.UnitTests.Infrastructure.MongoDb.EventHandlers
{
    public class PaymentProcessedMongoHandlerTest
    {
        private readonly Mock<IPaymentReportRepository> _paymentReportRepositoryMock;
        private readonly PaymentProcessedMongoHandler _sut;

        public PaymentProcessedMongoHandlerTest()
        {
            _paymentReportRepositoryMock = new Mock<IPaymentReportRepository>();
            var logger = new Mock<ILogger<PaymentProcessedMongoHandler>>().Object;
            _sut = new PaymentProcessedMongoHandler(_paymentReportRepositoryMock.Object, logger);
        }

        [Fact]
        public async Task Given_PaymentProcessedEvent_When_Handle_Then_Should_Insert_PaymentReport()
        {
            // Arrange
            var processedAt = DateTime.UtcNow;
            var domainEvent = new PaymentProcessedEvent(
                "test@example.com",
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                125m,
                PaymentStatus.Approved,
                processedAt);

            // Act
            await _sut.Handle(domainEvent, CancellationToken.None);

            // Assert
            _paymentReportRepositoryMock.Verify(repository => repository.InsertAsync(
                It.Is<PaymentReport>(report =>
                    report.PaymentId == domainEvent.PaymentId &&
                    report.UserId == domainEvent.UserId &&
                    report.GameId == domainEvent.GameId &&
                    report.Amount == domainEvent.Amount &&
                    report.Status == domainEvent.Status &&
                    report.ProcessedAt == processedAt),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Given_Repository_Failure_When_Handle_Then_Should_Rethrow()
        {
            // Arrange
            var expectedException = new InvalidOperationException("MongoDB insert failed");
            var domainEvent = new PaymentProcessedEvent(
                "test@example.com",
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                125m,
                PaymentStatus.Approved,
                DateTime.UtcNow);

            _paymentReportRepositoryMock
                .Setup(repository => repository.InsertAsync(It.IsAny<PaymentReport>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(expectedException);

            // Act
            var act = async () => await _sut.Handle(domainEvent, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("MongoDB insert failed");
        }
    }
}
