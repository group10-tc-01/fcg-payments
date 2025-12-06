using FCG.Payments.Application.Abstractions.Messaging;

namespace FCG.Payments.Application.UseCases.Wallets.Deposit
{
    public sealed record DepositCommand(Guid WalletId, decimal Amount) : ICommand<DepositResponse>;
}
