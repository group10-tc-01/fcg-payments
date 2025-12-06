namespace FCG.Payments.Application.UseCases.Wallets.Deposit
{
    public sealed record DepositResponse(Guid WalletId, decimal NewBalance);
}
