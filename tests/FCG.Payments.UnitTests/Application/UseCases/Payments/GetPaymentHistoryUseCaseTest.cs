using FCG.Payments.Application.UseCases.Payments.GetPaymentHistory;
using FCG.Payments.CommomTestUtilities.Builders.Payments;
using FCG.Payments.CommomTestUtilities.Builders.Payments.Repositories;
using FCG.Payments.Domain.Payments;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace FCG.Payments.UnitTests.Application.UseCases.Payments
{
    public class GetPaymentHistoryUseCaseTest
    {
        private readonly IReadOnlyPaymentRepository _readOnlyPaymentRepository;
        private readonly ILogger<GetPaymentHistoryUseCase> _logger;
        private readonly IGetPaymentHistoryUseCase _sut;

        public GetPaymentHistoryUseCaseTest()
        {
            _readOnlyPaymentRepository = ReadOnlyPaymentRepositoryBuilder.Build();
            _logger = new Mock<ILogger<GetPaymentHistoryUseCase>>().Object;
            _sut = new GetPaymentHistoryUseCase(_readOnlyPaymentRepository, _logger);
        }

        [Fact]
        public async Task Given_Valid_Request_When_GetPaymentHistoryIsCalled_Then_Should_Return_PagedList()
        {
            // Arrange
            var pageNumber = 1;
            var pageSize = 10;
            var payment1 = new PaymentBuilder().Build();
            var payment2 = new PaymentBuilder().Build();
            var payments = new List<Payment> { payment1, payment2 };
            var totalCount = 2;

            ReadOnlyPaymentRepositoryBuilder._mock
                .Setup(repo => repo.GetPaymentHistoryAsync(
                    pageNumber,
                    pageSize,
                    It.IsAny<PaymentStatus?>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((payments, totalCount));

            var request = new GetPaymentHistoryRequest(pageNumber, pageSize, null, null, null);

            // Act
            var response = await _sut.Handle(request, CancellationToken.None);

            // Assert
            response.Should().NotBeNull();
            response.Items.Should().HaveCount(2);
            response.TotalCount.Should().Be(totalCount);
            response.PageSize.Should().Be(pageSize);
        }

        [Fact]
        public async Task Given_Request_With_Status_Filter_When_GetPaymentHistoryIsCalled_Then_Should_Return_Filtered_Results()
        {
            // Arrange
            var pageNumber = 1;
            var pageSize = 10;
            var status = PaymentStatus.Approved;
            var payment = new PaymentBuilder().Build();
            var payments = new List<Payment> { payment };
            var totalCount = 1;

            ReadOnlyPaymentRepositoryBuilder._mock
                .Setup(repo => repo.GetPaymentHistoryAsync(
                    pageNumber,
                    pageSize,
                    status,
                    It.IsAny<DateTime?>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((payments, totalCount));

            var request = new GetPaymentHistoryRequest(pageNumber, pageSize, status, null, null);

            // Act
            var response = await _sut.Handle(request, CancellationToken.None);

            // Assert
            response.Should().NotBeNull();
            response.Items.Should().HaveCount(1);
            response.TotalCount.Should().Be(totalCount);
        }

        [Fact]
        public async Task Given_Request_With_Date_Range_When_GetPaymentHistoryIsCalled_Then_Should_Return_Filtered_Results()
        {
            // Arrange
            var pageNumber = 1;
            var pageSize = 10;
            var dateFrom = DateTime.UtcNow.AddDays(-7);
            var dateTo = DateTime.UtcNow;
            var payment = new PaymentBuilder().Build();
            var payments = new List<Payment> { payment };
            var totalCount = 1;

            ReadOnlyPaymentRepositoryBuilder._mock
                .Setup(repo => repo.GetPaymentHistoryAsync(
                    pageNumber,
                    pageSize,
                    It.IsAny<PaymentStatus?>(),
                    dateFrom,
                    dateTo,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((payments, totalCount));

            var request = new GetPaymentHistoryRequest(pageNumber, pageSize, null, dateFrom, dateTo);

            // Act
            var response = await _sut.Handle(request, CancellationToken.None);

            // Assert
            response.Should().NotBeNull();
            response.Items.Should().HaveCount(1);
            response.TotalCount.Should().Be(totalCount);
        }

        [Fact]
        public async Task Given_Empty_Results_When_GetPaymentHistoryIsCalled_Then_Should_Return_Empty_PagedList()
        {
            // Arrange
            var pageNumber = 1;
            var pageSize = 10;
            var payments = new List<Payment>();
            var totalCount = 0;

            ReadOnlyPaymentRepositoryBuilder._mock
                .Setup(repo => repo.GetPaymentHistoryAsync(
                    pageNumber,
                    pageSize,
                    It.IsAny<PaymentStatus?>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((payments, totalCount));

            var request = new GetPaymentHistoryRequest(pageNumber, pageSize, null, null, null);

            // Act
            var response = await _sut.Handle(request, CancellationToken.None);

            // Assert
            response.Should().NotBeNull();
            response.Items.Should().BeEmpty();
            response.TotalCount.Should().Be(0);
        }

        [Fact]
        public async Task Given_Valid_Request_When_GetPaymentHistoryIsCalled_Then_Response_Should_Have_All_Payment_Properties()
        {
            // Arrange
            var pageNumber = 1;
            var pageSize = 10;
            var userId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            var walletId = Guid.NewGuid();
            var amount = 100m;
            var payment = new PaymentBuilder().BuildWithParameters(Guid.NewGuid(), userId, gameId, walletId, amount);
            var payments = new List<Payment> { payment };
            var totalCount = 1;

            ReadOnlyPaymentRepositoryBuilder._mock
                .Setup(repo => repo.GetPaymentHistoryAsync(
                    pageNumber,
                    pageSize,
                    It.IsAny<PaymentStatus?>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((payments, totalCount));

            var request = new GetPaymentHistoryRequest(pageNumber, pageSize, null, null, null);

            // Act
            var response = await _sut.Handle(request, CancellationToken.None);

            // Assert
            var paymentResponse = response.Items.First();
            paymentResponse.Id.Should().Be(payment.Id);
            paymentResponse.UserId.Should().Be(payment.UserId);
            paymentResponse.GameId.Should().Be(payment.GameId);
            paymentResponse.WalletId.Should().Be(payment.WalletId);
            paymentResponse.Amount.Should().Be(payment.Amount);
            paymentResponse.Status.Should().Be(payment.Status);
            paymentResponse.FailureReason.Should().Be(payment.FailureReason);
            paymentResponse.ProcessedAt.Should().Be(payment.ProcessedAt);
            paymentResponse.CreatedAt.Should().Be(payment.CreatedAt);
        }

        [Fact]
        public void GetPaymentHistoryRequest_Should_Have_All_Properties()
        {
            // Arrange
            var pageNumber = 2;
            var pageSize = 20;
            var status = PaymentStatus.Pending;
            var dateFrom = DateTime.UtcNow.AddDays(-10);
            var dateTo = DateTime.UtcNow;

            // Act
            var request = new GetPaymentHistoryRequest(pageNumber, pageSize, status, dateFrom, dateTo);

            // Assert
            request.PageNumber.Should().Be(pageNumber);
            request.PageSize.Should().Be(pageSize);
            request.Status.Should().Be(status);
            request.DateFrom.Should().Be(dateFrom);
            request.DateTo.Should().Be(dateTo);
        }

        [Fact]
        public void GetPaymentHistoryResponse_Should_Have_All_Properties()
        {
            // Arrange
            var id = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            var walletId = Guid.NewGuid();
            var amount = 150m;
            var status = PaymentStatus.Approved;
            var failureReason = "Test reason";
            var processedAt = DateTime.UtcNow;
            var createdAt = DateTime.UtcNow.AddMinutes(-5);

            // Act
            var response = new GetPaymentHistoryResponse(
                id,
                userId,
                gameId,
                walletId,
                amount,
                status,
                failureReason,
                processedAt,
                createdAt);

            // Assert
            response.Id.Should().Be(id);
            response.UserId.Should().Be(userId);
            response.GameId.Should().Be(gameId);
            response.WalletId.Should().Be(walletId);
            response.Amount.Should().Be(amount);
            response.Status.Should().Be(status);
            response.FailureReason.Should().Be(failureReason);
            response.ProcessedAt.Should().Be(processedAt);
            response.CreatedAt.Should().Be(createdAt);
        }
    }
}
