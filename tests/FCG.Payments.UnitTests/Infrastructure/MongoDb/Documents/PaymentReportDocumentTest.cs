using FCG.Payments.Domain.Payments;
using FCG.Payments.Domain.Payments.Reports;
using FCG.Payments.Infrastructure.MongoDb.Documents;
using FluentAssertions;

namespace FCG.Payments.UnitTests.Infrastructure.MongoDb.Documents
{
    public class PaymentReportDocumentTest
    {
        [Fact]
        public void Given_PaymentReport_When_FromDomainIsCalled_Then_Should_Map_All_Properties()
        {
            // Arrange
            var report = new PaymentReport(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                125.50m,
                PaymentStatus.Approved,
                DateTime.UtcNow);

            // Act
            var document = PaymentReportDocument.FromDomain(report);

            // Assert
            document.PaymentId.Should().Be(report.PaymentId);
            document.UserId.Should().Be(report.UserId);
            document.GameId.Should().Be(report.GameId);
            document.Amount.Should().Be(report.Amount);
            document.Status.Should().Be(report.Status);
            document.ProcessedAt.Should().Be(report.ProcessedAt);
        }

        [Fact]
        public void Given_PaymentReportDocument_When_ToDomainIsCalled_Then_Should_Map_All_Properties()
        {
            // Arrange
            var document = new PaymentReportDocument
            {
                PaymentId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                GameId = Guid.NewGuid(),
                Amount = 85.75m,
                Status = PaymentStatus.Rejected,
                ProcessedAt = DateTime.UtcNow
            };

            // Act
            var report = document.ToDomain();

            // Assert
            report.PaymentId.Should().Be(document.PaymentId);
            report.UserId.Should().Be(document.UserId);
            report.GameId.Should().Be(document.GameId);
            report.Amount.Should().Be(document.Amount);
            report.Status.Should().Be(document.Status);
            report.ProcessedAt.Should().Be(document.ProcessedAt);
        }
    }
}
