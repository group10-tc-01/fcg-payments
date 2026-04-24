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
            var request = new GetPaymentReportRequest(1, 10);

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
            var request = new GetPaymentReportRequest(pageNumber, pageSize);

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(error => error.ErrorMessage == message);
        }
    }
}
