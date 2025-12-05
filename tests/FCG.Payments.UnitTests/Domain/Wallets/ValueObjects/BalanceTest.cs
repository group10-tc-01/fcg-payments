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
        public void Given_ValidAmount_When_Add_Then_ShouldIncreaseBalance()
        {
            // Arrange
            var balance = Balance.Create(100m);
            decimal amountToAdd = 50m;

            // Act
            var newBalance = balance.Add(amountToAdd);

            // Assert
            newBalance.Should().NotBeNull();
            newBalance.Value.Should().Be(150m);
        }

        [Fact]
        public void Given_NegativeAmount_When_Add_Then_ShouldThrowDomainException()
        {
            // Arrange
            var balance = Balance.Create(100m);
            decimal negativeAmount = -10m;
            var act = () => balance.Add(negativeAmount);

            // Act & Assert
            act.Should().Throw<DomainException>().WithMessage("Cannot add negative amount to balance");
        }

        [Fact]
        public void Given_ValidAmount_When_Subtract_Then_ShouldDecreaseBalance()
        {
            // Arrange
            var balance = Balance.Create(100m);
            decimal amountToSubtract = 30m;

            // Act
            var newBalance = balance.Subtract(amountToSubtract);

            // Assert
            newBalance.Should().NotBeNull();
            newBalance.Value.Should().Be(70m);
        }

        [Fact]
        public void Given_NegativeAmount_When_Subtract_Then_ShouldThrowDomainException()
        {
            // Arrange
            var balance = Balance.Create(100m);
            decimal negativeAmount = -10m;
            var act = () => balance.Subtract(negativeAmount);

            // Act & Assert
            act.Should().Throw<DomainException>().WithMessage("Cannot subtract negative amount from balance");
        }

        [Fact]
        public void Given_AmountGreaterThanBalance_When_Subtract_Then_ShouldThrowDomainException()
        {
            // Arrange
            var balance = Balance.Create(50m);
            decimal amountToSubtract = 100m;
            var act = () => balance.Subtract(amountToSubtract);

            // Act & Assert
            act.Should().Throw<DomainException>().WithMessage("Insufficient balance for this operation");
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
