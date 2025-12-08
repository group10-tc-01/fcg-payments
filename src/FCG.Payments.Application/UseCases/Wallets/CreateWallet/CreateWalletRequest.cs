using FCG.Payments.Application.Abstractions.Messaging;

namespace FCG.Payments.Application.UseCases.Wallets.CreateWallet
{
    public record CreateWalletRequest(Guid UserId) : ICommand<CreateWalletResponse>;
}
