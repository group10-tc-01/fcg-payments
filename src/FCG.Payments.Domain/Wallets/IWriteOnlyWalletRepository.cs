namespace FCG.Payments.Domain.Wallets
{
    public interface IWriteOnlyWalletRepository
    {
        Task AddAsync(Wallet wallet, CancellationToken cancellationToken = default);
        Task AddDepositAync(Guid walletId, decimal amount);
    }
}
