using FCG.Payments.Domain.Abstractions;
using FCG.Payments.Domain.Payments.Events;
using FCG.Payments.Domain.Payments.ValueObjects;
using FCG.Payments.Domain.Wallets;

namespace FCG.Payments.Domain.Payments
{
    public sealed class Payment : BaseEntity
    {
        public Guid UserId { get; private set; }
        public Guid GameId { get; private set; }
        public Guid WalletId { get; private set; }
        public Amount Amount { get; private set; } = null!;
        public PaymentStatus Status { get; private set; }
        public string? FailureReason { get; private set; }
        public DateTime? ProcessedAt { get; private set; }
        public Wallet Wallet { get; } = null!;

        public static Payment CreatePayment(Guid userId, Guid gameId, Guid walletId, decimal amount)
        {
            return new Payment(userId, gameId, walletId, amount);
        }

        public void Approve()
        {
            Status = PaymentStatus.Approved;
            ProcessedAt = DateTime.UtcNow;

            RaiseDomainEvent(new PaymentProcessedEvent(Id, UserId, GameId, Amount, Status, ProcessedAt.Value));
        }

        public void Reject(string reason)
        {
            Status = PaymentStatus.Rejected;
            FailureReason = reason;
            ProcessedAt = DateTime.UtcNow;

            RaiseDomainEvent(new PaymentProcessedEvent(Id, UserId, GameId, Amount, Status, ProcessedAt.Value));
        }

        private Payment(Guid userId, Guid gameId, Guid walletId, decimal amount) : base(Guid.NewGuid())
        {
            UserId = userId;
            GameId = gameId;
            WalletId = walletId;
            Amount = amount;
            Status = PaymentStatus.Pending;
        }

        private Payment() { }
    }
}
