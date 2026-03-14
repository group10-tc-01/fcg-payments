using FCG.Payments.Domain.Abstractions;
using FCG.Payments.Domain.Exceptions;
using FCG.Payments.Domain.Wallets.ValueObjects;

namespace FCG.Payments.Domain.Wallets
{
    public sealed class Wallet : BaseEntity, IAuditableEntity
    {
        public Guid UserId { get; private set; }
        public Balance Balance { get; private set; } = null!;

        #region Audits properties
        // Implementação explícita - expõe via interface, mas mantém protected set
        DateTime IAuditableEntity.CreatedAt { get => CreatedAt; set => CreatedAt = value; }
        DateTime? IAuditableEntity.UpdatedAt { get => UpdatedAt; set => UpdatedAt = value; }
        string IAuditableEntity.CreatedBy { get => CreatedBy; set => CreatedBy = value; }
        string? IAuditableEntity.UpdatedBy { get => UpdatedBy; set => UpdatedBy = value; }
        #endregion

        public static Wallet CreateWallet(Guid userId)
        {
            return new Wallet(userId);
        }

        public bool TryDebit(decimal amount)
        {
            if (amount <= 0)
                return false;

            var newValue = Balance - amount;

            if (newValue < 0)
                return false;

            Balance = newValue;
            UpdatedAt = DateTime.UtcNow;

            return true;
        }

        public void AddBalance(decimal amount)
        {
            if (amount < 0)
                throw new DomainException("Cannot add negative amount to balance");

            Balance += amount;
            UpdatedAt = DateTime.UtcNow;
        }

        private Wallet(Guid userId) : base(Guid.NewGuid())
        {
            UserId = userId;
            Balance = 1000m;
        }

        private Wallet() { }
    }
}
