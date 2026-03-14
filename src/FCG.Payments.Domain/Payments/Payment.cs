using FCG.Payments.Domain.Abstractions;
using FCG.Payments.Domain.Payments.Events;
using FCG.Payments.Domain.Payments.ValueObjects;
using FCG.Payments.Domain.Wallets;

namespace FCG.Payments.Domain.Payments
{
    public sealed class Payment : BaseEntity, IAuditableEntity
    {
        public string UserEmail { get; private set; } = null!;
        public Guid CorrelationId { get; private set; }
        public Guid UserId { get; private set; }
        public Guid GameId { get; private set; }
        public Guid WalletId { get; private set; }
        public Amount Amount { get; private set; } = null!;
        public PaymentStatus Status { get; private set; }
        public string? FailureReason { get; private set; }
        public DateTime? ProcessedAt { get; private set; }
        public Wallet Wallet { get; } = null!;

        #region Audits properties
        // Implementação explícita - expõe via interface, mas mantém protected set
        DateTime IAuditableEntity.CreatedAt { get => CreatedAt; set => CreatedAt = value; }
        DateTime? IAuditableEntity.UpdatedAt { get => UpdatedAt; set => UpdatedAt = value; }
        string IAuditableEntity.CreatedBy { get => CreatedBy; set => CreatedBy = value; }
        string? IAuditableEntity.UpdatedBy { get => UpdatedBy; set => UpdatedBy = value; }
        #endregion

        public static Payment CreatePayment(string userEmail, Guid correlationId, Guid userId, Guid gameId, Guid walletId, decimal amount)
        {
            return new Payment(userEmail, correlationId, userId, gameId, walletId, amount);
        }

        public void Approve()
        {
            Status = PaymentStatus.Approved;
            ProcessedAt = DateTime.UtcNow;

            RaiseDomainEvent(new PaymentProcessedEvent(UserEmail, CorrelationId, Id, UserId, GameId, Amount, Status, ProcessedAt.Value));
        }

        public void Reject(string reason)
        {
            Status = PaymentStatus.Rejected;
            FailureReason = reason;
            ProcessedAt = DateTime.UtcNow;

            RaiseDomainEvent(new PaymentProcessedEvent(UserEmail, CorrelationId, Id, UserId, GameId, Amount, Status, ProcessedAt.Value));
        }

        private Payment(string userEmail, Guid correlationId, Guid userId, Guid gameId, Guid walletId, decimal amount) : base(Guid.NewGuid())
        {
            UserEmail = userEmail;
            CorrelationId = correlationId;
            UserId = userId;
            GameId = gameId;
            WalletId = walletId;
            Amount = amount;
            Status = PaymentStatus.Pending;
        }

        private Payment() { }
    }
}
