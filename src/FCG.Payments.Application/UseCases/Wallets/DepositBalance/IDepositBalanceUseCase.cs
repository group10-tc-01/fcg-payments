using FCG.Payments.Application.Abstractions.Messaging;

namespace FCG.Payments.Application.UseCases.Wallets.DepositBalance
{
    public interface IDepositBalanceUseCase : ICommandHandler<DepositBalanceRequest, DepositBalanceResponse> { }
}
