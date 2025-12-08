namespace FCG.Payments.Domain.Wallets
{
    public interface IReadOnlyWalletRepository
    {
        Task<Wallet?> GetByIdAsync(Guid walletId, CancellationToken cancellationToken = default);
        Task<Wallet?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
