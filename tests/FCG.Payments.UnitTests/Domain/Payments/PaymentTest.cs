using FCG.Payments.CommomTestUtilities.Builders.Payments;
using FCG.Payments.Domain.Payments;
using FluentAssertions;

namespace FCG.Payments.UnitTests.Domain.Payments
{
    public class PaymentTest
    {
        [Fact]
        public void Given_ValidParameters_When_CreatePayment_Then_ShouldInstantiatePayment()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            var walletId = Guid.NewGuid();
            var amount = 100.50m;

            // Act
            var payment = Payment.CreatePayment("user@example.com", Guid.NewGuid(), userId, gameId, walletId, amount);

            // Assert
            payment.Should().NotBeNull();
            payment.Id.Should().NotBe(Guid.Empty);
            payment.UserId.Should().Be(userId);
            payment.GameId.Should().Be(gameId);
            payment.WalletId.Should().Be(walletId);
            payment.Amount.Value.Should().Be(amount);
            payment.Status.Should().Be(PaymentStatus.Pending);
            payment.FailureReason.Should().BeNull();
            payment.ProcessedAt.Should().BeNull();
            payment.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(30));
            payment.UpdatedAt?.TimeOfDay.Should().Be(TimeSpan.Zero);
        }

        [Fact]
        public void Given_PendingPayment_When_Approve_Then_ShouldChangeStatusToApproved()
        {
            // Arrange
            var payment = new PaymentBuilder().Build();

            // Act
            payment.Approve();

            // Assert
            payment.Status.Should().Be(PaymentStatus.Approved);
            payment.ProcessedAt.Should().NotBeNull();
            payment.ProcessedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(30));
            payment.FailureReason.Should().BeNull();
        }

        [Fact]
        public void Given_PendingPayment_When_Reject_Then_ShouldChangeStatusToRejected()
        {
            // Arrange
            var payment = new PaymentBuilder().Build();
            var failureReason = "Insufficient funds";

            // Act
            payment.Reject(failureReason);

            // Assert
            payment.Status.Should().Be(PaymentStatus.Rejected);
            payment.FailureReason.Should().Be(failureReason);
            payment.ProcessedAt.Should().NotBeNull();
            payment.ProcessedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(30));
        }

        [Fact]
        public void Given_DifferentParameters_When_CreatePayment_Then_ShouldCreateDifferentPayments()
        {
            // Arrange
            var userId1 = Guid.NewGuid();
            var userId2 = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            var walletId = Guid.NewGuid();
            var amount = 50m;

            // Act
            var payment1 = Payment.CreatePayment("user1@example.com", Guid.NewGuid(), userId1, gameId, walletId, amount);
            var payment2 = Payment.CreatePayment("user2@example.com", Guid.NewGuid(), userId2, gameId, walletId, amount);

            // Assert
            payment1.Id.Should().NotBe(payment2.Id);
            payment1.UserId.Should().Be(userId1);
            payment2.UserId.Should().Be(userId2);
        }

        [Fact]
        public void Given_Payment_When_GetDomainEvents_Then_ShouldReturnEmptyListByDefault()
        {
            // Arrange
            var payment = new PaymentBuilder().Build();

            // Act
            var domainEvents = payment.GetDomainEvents();

            // Assert
            domainEvents.Should().NotBeNull();
            domainEvents.Should().BeEmpty();
        }

        [Fact]
        public void Given_Payment_When_ClearDomainEvents_Then_ShouldRemoveAllEvents()
        {
            // Arrange
            var payment = new PaymentBuilder().Build();

            // Act
            payment.ClearDomainEvents();

            // Assert
            var domainEvents = payment.GetDomainEvents();
            domainEvents.Should().NotBeNull();
            domainEvents.Should().BeEmpty();
        }

        [Fact]
        public void Given_Payment_When_ApprovedTwice_Then_ProcessedAtShouldBeUpdated()
        {
            // Arrange
            var payment = new PaymentBuilder().Build();
            payment.Approve();
            var firstProcessedAt = payment.ProcessedAt;

            System.Threading.Thread.Sleep(100);

            // Act
            payment.Approve();

            // Assert
            payment.ProcessedAt.Should().BeAfter(firstProcessedAt!.Value);
        }

        [Fact]
        public void Given_Payment_When_RejectedWithEmptyReason_Then_FailureReasonShouldBeSet()
        {
            // Arrange
            var payment = new PaymentBuilder().Build();
            var emptyReason = string.Empty;

            // Act
            payment.Reject(emptyReason);

            // Assert
            payment.Status.Should().Be(PaymentStatus.Rejected);
            payment.FailureReason.Should().Be(emptyReason);
            payment.ProcessedAt.Should().NotBeNull();
        }

        [Fact]
        public void Given_ValidPayment_When_CheckProperties_Then_AllPropertiesShouldBeAccessible()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            var walletId = Guid.NewGuid();
            var amount = 250m;
            var payment = Payment.CreatePayment("user@example.com", Guid.NewGuid(), userId, gameId, walletId, amount);

            // Act & Assert
            payment.Id.Should().NotBe(Guid.Empty);
            payment.UserId.Should().Be(userId);
            payment.GameId.Should().Be(gameId);
            payment.WalletId.Should().Be(walletId);
            payment.Amount.Value.Should().Be(amount);
            payment.Status.Should().Be(PaymentStatus.Pending);
            payment.CreatedAt.Should().NotBe(default(DateTime));
            payment.UpdatedAt.Should().BeNull();
            payment.ProcessedAt.Should().BeNull();
            payment.FailureReason.Should().BeNull();
        }
    }
}
