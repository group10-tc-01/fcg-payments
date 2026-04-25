using FCG.Payments.Application.Abstractions.Reports;
using FCG.Payments.Application.UseCases.Payments;
using FCG.Payments.Application.UseCases.Payments.ExportPaymentReport;
using FCG.Payments.Application.UseCases.Payments.GetPaymentReport;
using FCG.Payments.Domain.Payments;
using FCG.Payments.Domain.Payments.Reports;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace FCG.Payments.UnitTests.Application.UseCases.Payments
{
    public class ExportPaymentReportUseCaseTest
    {
        private readonly Mock<IPaymentReportRepository> _paymentReportRepositoryMock;
        private readonly Mock<IPaymentReportPdfGenerator> _paymentReportPdfGeneratorMock;
        private readonly ExportPaymentReportUseCase _sut;

        public ExportPaymentReportUseCaseTest()
        {
            _paymentReportRepositoryMock = new Mock<IPaymentReportRepository>();
            _paymentReportPdfGeneratorMock = new Mock<IPaymentReportPdfGenerator>();
            var logger = new Mock<ILogger<ExportPaymentReportUseCase>>().Object;
            var options = Options.Create(new PaymentReportSettings
            {
                PdfExportMaxRecords = 100
            });

            _sut = new ExportPaymentReportUseCase(
                _paymentReportRepositoryMock.Object,
                _paymentReportPdfGeneratorMock.Object,
                options,
                logger);
        }

        [Fact]
        public async Task Given_Filtered_Request_When_ExportPaymentReportIsCalled_Then_Should_Return_Filtered_Items_And_Summary()
        {
            // Arrange
            var status = PaymentReportStatusFilter.Approved;
            var dateFrom = new DateTime(2026, 04, 01, 0, 0, 0, DateTimeKind.Utc);
            var dateTo = new DateTime(2026, 04, 25, 0, 0, 0, DateTimeKind.Utc);
            var userId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            var reports = new List<PaymentReport>
            {
                new(Guid.NewGuid(), userId, gameId, 100m, PaymentStatus.Approved, DateTime.UtcNow)
            };
            var summary = new PaymentReportSummary(1, 1, 0, 100m, 0m);
            PaymentReportFilter? summaryFilter = null;
            PaymentReportFilter? exportFilter = null;

            _paymentReportRepositoryMock
                .Setup(repository => repository.GetSummaryAsync(It.IsAny<PaymentReportFilter>(), It.IsAny<CancellationToken>()))
                .Callback<PaymentReportFilter, CancellationToken>((filter, _) => summaryFilter = filter)
                .ReturnsAsync(summary);

            _paymentReportRepositoryMock
                .Setup(repository => repository.GetAsync(It.IsAny<PaymentReportFilter>(), 100, It.IsAny<CancellationToken>()))
                .Callback<PaymentReportFilter, int, CancellationToken>((filter, _, _) => exportFilter = filter)
                .ReturnsAsync(reports);
            _paymentReportPdfGeneratorMock
                .Setup(generator => generator.Generate(It.IsAny<PaymentReportPdfData>()))
                .Returns([0x25, 0x50, 0x44, 0x46]);

            var request = new ExportPaymentReportRequest(status, dateFrom, dateTo, userId, gameId);

            // Act
            var response = await _sut.Handle(request, CancellationToken.None);

            // Assert
            response.ContentType.Should().Be("application/pdf");
            response.FileName.Should().StartWith("payment-reports-");
            response.Content.Should().Equal(0x25, 0x50, 0x44, 0x46);
            summaryFilter.Should().Be(new PaymentReportFilter(PaymentStatus.Approved, dateFrom, dateTo, userId, gameId));
            exportFilter.Should().Be(summaryFilter);
            _paymentReportPdfGeneratorMock.Verify(generator => generator.Generate(
                It.Is<PaymentReportPdfData>(data =>
                    data.Items.Count == 1 &&
                    data.Summary == new GetPaymentReportSummaryResponse(1, 1, 0, 100m, 0m) &&
                    data.Filter == summaryFilter)),
                Times.Once);
        }

        [Fact]
        public async Task Given_Result_Count_Greater_Than_Limit_When_ExportPaymentReportIsCalled_Then_Should_Export_With_TotalMatched()
        {
            // Arrange
            var totalMatched = 150;
            var limitedReports = Enumerable.Range(0, 100)
                .Select(_ => new PaymentReport(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 10m, PaymentStatus.Approved, DateTime.UtcNow))
                .ToList();

            _paymentReportRepositoryMock
                .Setup(repository => repository.GetSummaryAsync(It.IsAny<PaymentReportFilter>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PaymentReportSummary(totalMatched, totalMatched, 0, totalMatched * 10m, 0m));

            _paymentReportRepositoryMock
                .Setup(repository => repository.GetAsync(It.IsAny<PaymentReportFilter>(), 100, It.IsAny<CancellationToken>()))
                .ReturnsAsync(limitedReports);

            _paymentReportPdfGeneratorMock
                .Setup(generator => generator.Generate(It.IsAny<PaymentReportPdfData>()))
                .Returns([0x25, 0x50, 0x44, 0x46]);

            var request = new ExportPaymentReportRequest(null, null, null, null, null);

            // Act
            var response = await _sut.Handle(request, CancellationToken.None);

            // Assert
            response.ContentType.Should().Be("application/pdf");
            _paymentReportPdfGeneratorMock.Verify(generator => generator.Generate(
                It.Is<PaymentReportPdfData>(data =>
                    data.Items.Count == 100 &&
                    data.TotalMatched == totalMatched)),
                Times.Once);
        }
    }
}
