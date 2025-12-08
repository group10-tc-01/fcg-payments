using FCG.Payments.Application.UseCases.Wallets.DepositBalance;
using FluentAssertions;

namespace FCG.Payments.UnitTests.Application.UseCases.Wallets
{
    public class DepositBalanceRequestValidatorTest
    {
        private readonly DepositBalanceRequestValidator _validator;

        public DepositBalanceRequestValidatorTest()
        {
            _validator = new DepositBalanceRequestValidator();
        }

        [Fact]
        public void Given_Valid_Amount_When_Validated_Then_Should_Pass()
        {
            // Arrange
            var request = new DepositBalanceRequest(Guid.NewGuid(), 100m);

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Given_Zero_Amount_When_Validated_Then_Should_Fail()
        {
            // Arrange
            var request = new DepositBalanceRequest(Guid.NewGuid(), 0m);

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle();
            result.Errors[0].ErrorMessage.Should().Be("Amount must be greater than zero");
            result.Errors[0].PropertyName.Should().Be("Amount");
        }

        [Fact]
        public void Given_Negative_Amount_When_Validated_Then_Should_Fail()
        {
            // Arrange
            var request = new DepositBalanceRequest(Guid.NewGuid(), -50m);

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle();
            result.Errors[0].ErrorMessage.Should().Be("Amount must be greater than zero");
            result.Errors[0].PropertyName.Should().Be("Amount");
        }

        [Theory]
        [InlineData(0.01)]
        [InlineData(1)]
        [InlineData(100)]
        [InlineData(1000)]
        [InlineData(999999.99)]
        public void Given_Valid_Positive_Amounts_When_Validated_Then_Should_Pass(decimal amount)
        {
            // Arrange
            var request = new DepositBalanceRequest(Guid.NewGuid(), amount);

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Theory]
        [InlineData(-0.01)]
        [InlineData(-1)]
        [InlineData(-100)]
        [InlineData(-1000)]
        public void Given_Invalid_Negative_Amounts_When_Validated_Then_Should_Fail(decimal amount)
        {
            // Arrange
            var request = new DepositBalanceRequest(Guid.NewGuid(), amount);

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle();
            result.Errors[0].PropertyName.Should().Be("Amount");
        }
    }
}
