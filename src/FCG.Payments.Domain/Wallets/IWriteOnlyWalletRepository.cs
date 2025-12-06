namespace FCG.Payments.Domain.Wallets
{
    public interface IWriteOnlyWalletRepository
    {
        public Task AddDepositAsync(Guid walletId, decimal amount);
    }
}
