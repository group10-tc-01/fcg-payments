using FCG.Payments.Application.UseCases.Payments;
using FCG.Payments.Application.UseCases.Payments.GetPaymentReport;
using FluentAssertions;

namespace FCG.Payments.UnitTests.Application.UseCases.Payments
{
    public class GetPaymentReportRequestValidatorTest
    {
        private readonly GetPaymentReportRequestValidator _validator = new();

        [Fact]
        public void Given_Valid_Request_When_Validate_Then_Should_Be_Valid()
        {
            // Arrange
            var request = new GetPaymentReportRequest(
                1,
                10,
                PaymentReportStatusFilter.Approved,
                DateTime.UtcNow.AddDays(-1),
                DateTime.UtcNow,
                Guid.NewGuid(),
                Guid.NewGuid());

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineData(0, 10, "PageNumber must be greater than zero")]
        [InlineData(1, 0, "PageSize must be greater than zero")]
        [InlineData(1, 51, "PageSize must be less than or equal to 50")]
        public void Given_Invalid_Request_When_Validate_Then_Should_Return_Error(int pageNumber, int pageSize, string message)
        {
            // Arrange
            var request = new GetPaymentReportRequest(pageNumber, pageSize, null, null, null, null, null);

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(error => error.ErrorMessage == message);
        }

        [Fact]
        public void Given_DateFrom_Greater_Than_DateTo_When_Validate_Then_Should_Return_Error()
        {
            // Arrange
            var request = new GetPaymentReportRequest(
                1,
                10,
                null,
                new DateTime(2026, 04, 25, 12, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 04, 24, 12, 0, 0, DateTimeKind.Utc),
                null,
                null);

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(error => error.ErrorMessage == "DateFrom must be less than or equal to DateTo");
        }
    }
}
