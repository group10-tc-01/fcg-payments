using FCG.Payments.Application.UseCases.Payments.GetPaymentHistory;
using FCG.Payments.Domain.Payments;
using FluentAssertions;

namespace FCG.Payments.UnitTests.Application.UseCases.Payments
{
    public class GetPaymentHistoryRequestValidatorTest
    {
        private readonly GetPaymentHistoryRequestValidator _validator;

        public GetPaymentHistoryRequestValidatorTest()
        {
            _validator = new GetPaymentHistoryRequestValidator();
        }

        [Fact]
        public void Given_Valid_Request_When_Validated_Then_Should_Pass()
        {
            // Arrange
            var request = new GetPaymentHistoryRequest(1, 10, null, null, null);

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Given_Zero_PageNumber_When_Validated_Then_Should_Fail()
        {
            // Arrange
            var request = new GetPaymentHistoryRequest(0, 10, null, null, null);

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "PageNumber");
            result.Errors.Should().Contain(e => e.ErrorMessage == "PageNumber must be greater than zero");
        }

        [Fact]
        public void Given_Negative_PageNumber_When_Validated_Then_Should_Fail()
        {
            // Arrange
            var request = new GetPaymentHistoryRequest(-1, 10, null, null, null);

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "PageNumber");
            result.Errors.Should().Contain(e => e.ErrorMessage == "PageNumber must be greater than zero");
        }

        [Fact]
        public void Given_Zero_PageSize_When_Validated_Then_Should_Fail()
        {
            // Arrange
            var request = new GetPaymentHistoryRequest(1, 0, null, null, null);

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "PageSize");
            result.Errors.Should().Contain(e => e.ErrorMessage == "PageSize must be greater than zero");
        }

        [Fact]
        public void Given_Negative_PageSize_When_Validated_Then_Should_Fail()
        {
            // Arrange
            var request = new GetPaymentHistoryRequest(1, -10, null, null, null);

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "PageSize");
            result.Errors.Should().Contain(e => e.ErrorMessage == "PageSize must be greater than zero");
        }

        [Fact]
        public void Given_PageSize_Greater_Than_50_When_Validated_Then_Should_Fail()
        {
            // Arrange
            var request = new GetPaymentHistoryRequest(1, 51, null, null, null);

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "PageSize");
            result.Errors.Should().Contain(e => e.ErrorMessage == "PageSize must be less than or equal to 50");
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(25)]
        [InlineData(50)]
        public void Given_Valid_PageSize_When_Validated_Then_Should_Pass(int pageSize)
        {
            // Arrange
            var request = new GetPaymentHistoryRequest(1, pageSize, null, null, null);

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Given_DateFrom_Greater_Than_DateTo_When_Validated_Then_Should_Fail()
        {
            // Arrange
            var dateFrom = DateTime.UtcNow;
            var dateTo = DateTime.UtcNow.AddDays(-7);
            var request = new GetPaymentHistoryRequest(1, 10, null, dateFrom, dateTo);

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "DateFrom");
            result.Errors.Should().Contain(e => e.ErrorMessage == "DateFrom must be less than or equal to DateTo");
        }

        [Fact]
        public void Given_DateFrom_Equal_To_DateTo_When_Validated_Then_Should_Pass()
        {
            // Arrange
            var date = DateTime.UtcNow;
            var request = new GetPaymentHistoryRequest(1, 10, null, date, date);

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Given_DateFrom_Less_Than_DateTo_When_Validated_Then_Should_Pass()
        {
            // Arrange
            var dateFrom = DateTime.UtcNow.AddDays(-7);
            var dateTo = DateTime.UtcNow;
            var request = new GetPaymentHistoryRequest(1, 10, null, dateFrom, dateTo);

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Given_Only_DateFrom_When_Validated_Then_Should_Pass()
        {
            // Arrange
            var dateFrom = DateTime.UtcNow.AddDays(-7);
            var request = new GetPaymentHistoryRequest(1, 10, null, dateFrom, null);

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Given_Only_DateTo_When_Validated_Then_Should_Pass()
        {
            // Arrange
            var dateTo = DateTime.UtcNow;
            var request = new GetPaymentHistoryRequest(1, 10, null, null, dateTo);

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Theory]
        [InlineData(PaymentStatus.Pending)]
        [InlineData(PaymentStatus.Approved)]
        [InlineData(PaymentStatus.Rejected)]
        public void Given_Valid_Status_When_Validated_Then_Should_Pass(PaymentStatus status)
        {
            // Arrange
            var request = new GetPaymentHistoryRequest(1, 10, status, null, null);

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Given_Null_Status_When_Validated_Then_Should_Pass()
        {
            // Arrange
            var request = new GetPaymentHistoryRequest(1, 10, null, null, null);

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Given_Multiple_Invalid_Fields_When_Validated_Then_Should_Return_All_Errors()
        {
            // Arrange
            var request = new GetPaymentHistoryRequest(0, 100, null, DateTime.UtcNow, DateTime.UtcNow.AddDays(-7));

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCountGreaterThan(1);
            result.Errors.Should().Contain(e => e.PropertyName == "PageNumber");
            result.Errors.Should().Contain(e => e.PropertyName == "PageSize");
            result.Errors.Should().Contain(e => e.PropertyName == "DateFrom");
        }
    }
}
