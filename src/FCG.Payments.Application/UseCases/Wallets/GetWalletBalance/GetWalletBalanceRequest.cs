using FCG.Payments.Application.Abstractions.Messaging;

namespace FCG.Payments.Application.UseCases.Wallets.GetWalletBalance
{
    public record GetWalletBalanceRequest(Guid Id) : ICommand<GetWalletBalanceResponse>;
}
