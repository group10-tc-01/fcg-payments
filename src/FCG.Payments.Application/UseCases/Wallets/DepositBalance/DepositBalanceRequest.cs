using FCG.Payments.Application.Abstractions.Messaging;

namespace FCG.Payments.Application.UseCases.Wallets.DepositBalance
{
    public record DepositBalanceRequest(Guid Id, decimal Amount) : ICommand<DepositBalanceResponse>;
}
