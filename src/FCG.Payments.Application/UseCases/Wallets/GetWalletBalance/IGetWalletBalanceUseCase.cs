using FCG.Payments.Application.Abstractions.Messaging;

namespace FCG.Payments.Application.UseCases.Wallets.GetWalletBalance
{
    public interface IGetWalletBalanceUseCase : ICommandHandler<GetWalletBalanceRequest, GetWalletBalanceResponse> { }
}
