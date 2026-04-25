using FCG.Payments.Application.Abstractions.Reports;
using FCG.Payments.Application.UseCases.Payments.ExportPaymentReport;
using FCG.Payments.Application.UseCases.Payments.GetPaymentReport;
using FCG.Payments.Domain.Payments;
using FCG.Payments.Domain.Payments.Reports;
using FCG.Payments.Infrastructure.Pdf.Reports;
using FluentAssertions;

namespace FCG.Payments.UnitTests.WebApi.Reports
{
    public class PaymentReportPdfGeneratorTest
    {
        [Fact]
        public void Given_Report_Data_When_GenerateIsCalled_Then_Should_Return_Pdf_Bytes()
        {
            // Arrange
            var generator = new PaymentReportPdfGenerator();
            var response = new PaymentReportPdfData(
                [
                    new GetPaymentReportItemResponse(
                        Guid.NewGuid(),
                        Guid.NewGuid(),
                        Guid.NewGuid(),
                        100m,
                        PaymentStatus.Approved,
                        DateTime.UtcNow)
                ],
                new GetPaymentReportSummaryResponse(1, 1, 0, 100m, 0m),
                new PaymentReportFilter(PaymentStatus.Approved, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow, null, null),
                DateTime.UtcNow,
                TotalMatched: 1);

            // Act
            var bytes = generator.Generate(response);

            // Assert
            bytes.Should().NotBeEmpty();
            bytes.Take(4).Should().Equal((byte)'%', (byte)'P', (byte)'D', (byte)'F');
        }

        [Fact]
        public void Given_Empty_Report_Data_When_GenerateIsCalled_Then_Should_Return_Pdf_Bytes()
        {
            // Arrange
            var generator = new PaymentReportPdfGenerator();
            var response = new PaymentReportPdfData(
                [],
                new GetPaymentReportSummaryResponse(0, 0, 0, 0m, 0m),
                PaymentReportFilter.Empty,
                DateTime.UtcNow,
                TotalMatched: 0);

            // Act
            var bytes = generator.Generate(response);

            // Assert
            bytes.Should().NotBeEmpty();
            bytes.Take(4).Should().Equal((byte)'%', (byte)'P', (byte)'D', (byte)'F');
        }
    }
}
