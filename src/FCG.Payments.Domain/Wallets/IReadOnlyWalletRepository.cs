namespace FCG.Payments.Domain.Wallets
{
    public interface IReadOnlyWalletRepository
    {
        Task<Wallet?> GetByIdAsync(Guid walletId, CancellationToken cancellationToken = default);
    }
}
