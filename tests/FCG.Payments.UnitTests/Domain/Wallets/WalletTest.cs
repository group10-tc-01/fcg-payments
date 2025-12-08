using FCG.Payments.CommomTestUtilities.Builders.Wallets;
using FCG.Payments.Domain.Wallets;
using FluentAssertions;

namespace FCG.Payments.UnitTests.Domain.Wallets
{
    public class WalletTest
    {
        [Fact]
        public void Given_ValidUserId_When_CreateWallet_Then_ShouldInstantiateWallet()
        {
            // Arrange
            var userId = Guid.NewGuid();

            // Act
            var wallet = Wallet.CreateWallet(userId);

            // Assert
            wallet.Should().NotBeNull();
            wallet.Id.Should().NotBe(Guid.Empty);
            wallet.UserId.Should().Be(userId);
            wallet.Balance.Should().NotBeNull();
            wallet.Balance.Value.Should().Be(1000m);
            wallet.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(30));
            wallet.UpdatedAt.TimeOfDay.Should().Be(TimeSpan.Zero);
        }

        [Fact]
        public void Given_DifferentUserIds_When_CreateWallet_Then_ShouldCreateDifferentWallets()
        {
            // Arrange
            var userId1 = Guid.NewGuid();
            var userId2 = Guid.NewGuid();

            // Act
            var wallet1 = Wallet.CreateWallet(userId1);
            var wallet2 = Wallet.CreateWallet(userId2);

            // Assert
            wallet1.Id.Should().NotBe(wallet2.Id);
            wallet1.UserId.Should().Be(userId1);
            wallet2.UserId.Should().Be(userId2);
        }

        [Fact]
        public void Given_NewWallet_When_Created_Then_ShouldHaveDefaultBalance()
        {
            // Arrange & Act
            var wallet = new WalletBuilder().Build();

            // Assert
            wallet.Balance.Should().NotBeNull();
            wallet.Balance.Value.Should().Be(1000m);
        }

        [Fact]
        public void Given_Wallet_When_GetDomainEvents_Then_ShouldReturnEmptyListByDefault()
        {
            // Arrange
            var wallet = new WalletBuilder().Build();

            // Act
            var domainEvents = wallet.GetDomainEvents();

            // Assert
            domainEvents.Should().NotBeNull();
            domainEvents.Should().BeEmpty();
        }

        [Fact]
        public void Given_Wallet_When_ClearDomainEvents_Then_ShouldRemoveAllEvents()
        {
            // Arrange
            var wallet = new WalletBuilder().Build();

            // Act
            wallet.ClearDomainEvents();

            // Assert
            var domainEvents = wallet.GetDomainEvents();
            domainEvents.Should().NotBeNull();
            domainEvents.Should().BeEmpty();
        }

        [Fact]
        public void Given_ValidWallet_When_CheckProperties_Then_AllPropertiesShouldBeAccessible()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var wallet = Wallet.CreateWallet(userId);

            // Act & Assert
            wallet.Id.Should().NotBe(Guid.Empty);
            wallet.UserId.Should().Be(userId);
            wallet.Balance.Should().NotBeNull();
            wallet.CreatedAt.Should().NotBe(default(DateTime));
            wallet.UpdatedAt.Should().Be(default(DateTime));
        }

        [Fact]
        public void Given_ValidAmount_When_TryDebit_ShouldReturnTrue()
        {
            // Arrange
            var wallet = new WalletBuilder().Build();
            var amountToDebit = 200m;

            // Act
            var result = wallet.TryDebit(amountToDebit);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void Given_InvalidAmount_When_TryDebit_ShouldReturnFalse()
        {
            // Arrange
            var wallet = new WalletBuilder().Build();
            var amountToDebit = -4m;

            // Act
            var result = wallet.TryDebit(amountToDebit);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void Given_InsufficientBalance_When_TryDebit_ShouldReturnFalse()
        {
            // Arrange
            var wallet = new WalletBuilder().Build();
            var amountToDebit = 2000m;

            // Act
            var result = wallet.TryDebit(amountToDebit);

            // Assert
            result.Should().BeFalse();
        }
    }
}
