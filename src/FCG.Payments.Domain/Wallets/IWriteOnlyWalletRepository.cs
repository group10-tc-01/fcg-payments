namespace FCG.Payments.Domain.Wallets
{
    public interface IWriteOnlyWalletRepository
    {
        public Task AddDepositAync(Guid walletId, decimal amount);
    }
}
