namespace FCG.Payments.Domain.Wallets
{
    public interface IReadOnlyWalletRepository
    {
        public Task<Wallet?> GetByIdAsync(Guid walletId, CancellationToken cancellationToken = default);
    }
}
