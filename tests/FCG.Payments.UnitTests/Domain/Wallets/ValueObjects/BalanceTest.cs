using FCG.Payments.Domain.Exceptions;
using FCG.Payments.Domain.Wallets.ValueObjects;
using FluentAssertions;

namespace FCG.Payments.UnitTests.Domain.Wallets.ValueObjects
{
    public class BalanceTest
    {
        [Fact]
        public void Given_ValidBalance_When_Create_Then_ShouldCreateSuccessfully()
        {
            // Arrange
            decimal validBalance = 100.50m;

            // Act
            var balance = Balance.Create(validBalance);

            // Assert
            balance.Should().NotBeNull();
            balance.Value.Should().Be(validBalance);
        }

        [Fact]
        public void Given_ZeroBalance_When_Create_Then_ShouldThrowDomainException()
        {
            // Arrange
            decimal zeroBalance = 0m;

            // Act
            var act = () => Balance.Create(zeroBalance);


            // Assert
            act.Should().Throw<DomainException>().WithMessage("Balance cannot be negative or zero");
        }

        [Fact]
        public void Given_NegativeBalance_When_Create_Then_ShouldThrowDomainException()
        {
            // Arrange
            decimal negativeBalance = -10.00m;
            var act = () => Balance.Create(negativeBalance);

            // Act & Assert
            act.Should().Throw<DomainException>().WithMessage("Balance cannot be negative or zero");
        }

        [Fact]
        public void Given_Balance_When_ImplicitConversionToDecimal_Then_ShouldReturnValue()
        {
            // Arrange
            var balance = Balance.Create(100m);

            // Act
            decimal value = balance;

            // Assert
            value.Should().Be(100m);
        }

        [Fact]
        public void Given_DecimalValue_When_ImplicitConversionToBalance_Then_ShouldCreateBalance()
        {
            // Arrange
            decimal value = 100m;

            // Act
            Balance balance = value;

            // Assert
            balance.Should().NotBeNull();
            balance.Value.Should().Be(100m);
        }
    }
}
