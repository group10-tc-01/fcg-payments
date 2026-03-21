using FCG.Payments.Domain.Exceptions;
using FCG.Payments.Domain.Payments.ValueObjects;
using FluentAssertions;

namespace FCG.Payments.UnitTests.Domain.Payments.ValueObjects
{
    public class AmountTest
    {
        [Fact]
        public void Given_ValidAmount_When_Create_Then_ShouldCreateSuccessfully()
        {
            // Arrange
            decimal validAmount = 100.50m;

            // Act
            var amount = Amount.Create(validAmount);

            // Assert
            amount.Should().NotBeNull();
            amount.Value.Should().Be(validAmount);
        }

        [Fact]
        public void Given_ZeroAmount_When_Create_Then_ShouldThrowDomainException()
        {
            // Arrange
            decimal zeroAmount = 0m;
            var act = () => Amount.Create(zeroAmount);

            // Act & Assert
            act.Should().Throw<DomainException>().WithMessage("Amount cannot be negative or zero");
        }

        [Fact]
        public void Given_NegativeAmount_When_Create_Then_ShouldThrowDomainException()
        {
            // Arrange
            decimal negativeAmount = -10.00m;
            var act = () => Amount.Create(negativeAmount);

            // Act & Assert
            act.Should().Throw<DomainException>().WithMessage("Amount cannot be negative or zero");
        }

        [Fact]
        public void Given_Amount_When_ImplicitConversionToDecimal_Then_ShouldReturnValue()
        {
            // Arrange
            var amount = Amount.Create(100m);

            // Act
            decimal value = amount;

            // Assert
            value.Should().Be(100m);
        }

        [Fact]
        public void Given_DecimalValue_When_ImplicitConversionToAmount_Then_ShouldCreateAmount()
        {
            // Arrange
            decimal value = 100m;

            // Act
            Amount amount = value;

            // Assert
            amount.Should().NotBeNull();
            amount.Value.Should().Be(100m);
        }

        [Fact]
        public void Given_Amount_When_ToString_Then_ShouldReturnFormattedValue()
        {
            // Arrange
            var amount = Amount.Create(1234.56m);

            // Act
            string result = amount.ToString();

            // Assert
            result.Should().BeOneOf("1234.56", "1234,56");
        }

        [Fact]
        public void Given_AmountWithManyDecimals_When_ToString_Then_ShouldReturnTwoDecimalPlaces()
        {
            // Arrange
            var amount = Amount.Create(1234.5m);

            // Act
            string result = amount.ToString();

            // Assert
            result.Should().BeOneOf("1234,50", "1234.50");
        }
    }
}
