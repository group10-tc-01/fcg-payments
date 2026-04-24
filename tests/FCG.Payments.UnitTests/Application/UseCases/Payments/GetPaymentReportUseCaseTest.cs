using FCG.Payments.Application.UseCases.Payments.GetPaymentReport;
using FCG.Payments.Domain.Payments;
using FCG.Payments.Domain.Payments.Reports;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace FCG.Payments.UnitTests.Application.UseCases.Payments
{
    public class GetPaymentReportUseCaseTest
    {
        private readonly Mock<IPaymentReportRepository> _paymentReportRepositoryMock;
        private readonly IGetPaymentReportUseCase _sut;

        public GetPaymentReportUseCaseTest()
        {
            _paymentReportRepositoryMock = new Mock<IPaymentReportRepository>();
            var logger = new Mock<ILogger<GetPaymentReportUseCase>>().Object;
            _sut = new GetPaymentReportUseCase(_paymentReportRepositoryMock.Object, logger);
        }

        [Fact]
        public async Task Given_Valid_Request_When_GetPaymentReportIsCalled_Then_Should_Return_Paged_Report_With_Summary()
        {
            // Arrange
            var pageNumber = 1;
            var pageSize = 10;
            var processedAt = DateTime.UtcNow;
            var reports = new List<PaymentReport>
            {
                new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 100m, PaymentStatus.Approved, processedAt),
                new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 50m, PaymentStatus.Rejected, processedAt.AddMinutes(-1))
            };
            var summary = new PaymentReportSummary(2, 1, 1, 100m, 50m);

            _paymentReportRepositoryMock
                .Setup(repository => repository.GetPagedAsync(pageNumber, pageSize, It.IsAny<CancellationToken>()))
                .ReturnsAsync((reports, reports.Count));

            _paymentReportRepositoryMock
                .Setup(repository => repository.GetSummaryAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(summary);

            var request = new GetPaymentReportRequest(pageNumber, pageSize);

            // Act
            var response = await _sut.Handle(request, CancellationToken.None);

            // Assert
            response.Should().NotBeNull();
            response.Items.Should().HaveCount(2);
            response.CurrentPage.Should().Be(pageNumber);
            response.PageSize.Should().Be(pageSize);
            response.TotalCount.Should().Be(2);
            response.TotalPages.Should().Be(1);
            response.Summary.Should().BeEquivalentTo(new GetPaymentReportSummaryResponse(2, 1, 1, 100m, 50m));

            var firstItem = response.Items.First();
            firstItem.PaymentId.Should().Be(reports[0].PaymentId);
            firstItem.UserId.Should().Be(reports[0].UserId);
            firstItem.GameId.Should().Be(reports[0].GameId);
            firstItem.Amount.Should().Be(reports[0].Amount);
            firstItem.Status.Should().Be(reports[0].Status);
            firstItem.ProcessedAt.Should().Be(reports[0].ProcessedAt);

            _paymentReportRepositoryMock.Verify(
                repository => repository.GetPagedAsync(pageNumber, pageSize, It.IsAny<CancellationToken>()),
                Times.Once);
            _paymentReportRepositoryMock.Verify(
                repository => repository.GetSummaryAsync(It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Given_Empty_Report_When_GetPaymentReportIsCalled_Then_Should_Return_Empty_Page_And_Summary()
        {
            // Arrange
            var pageNumber = 1;
            var pageSize = 10;
            var summary = new PaymentReportSummary(0, 0, 0, 0m, 0m);

            _paymentReportRepositoryMock
                .Setup(repository => repository.GetPagedAsync(pageNumber, pageSize, It.IsAny<CancellationToken>()))
                .ReturnsAsync(([], 0));

            _paymentReportRepositoryMock
                .Setup(repository => repository.GetSummaryAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(summary);

            var request = new GetPaymentReportRequest(pageNumber, pageSize);

            // Act
            var response = await _sut.Handle(request, CancellationToken.None);

            // Assert
            response.Items.Should().BeEmpty();
            response.TotalCount.Should().Be(0);
            response.TotalPages.Should().Be(0);
            response.Summary.TotalPayments.Should().Be(0);
        }
    }
}
