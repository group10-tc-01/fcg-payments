using FCG.Payments.Application.Abstractions.Messaging;

namespace FCG.Payments.Application.UseCases.Wallets.CreateWallet
{
    public interface ICreateWalletUseCase : ICommandHandler<CreateWalletRequest, CreateWalletResponse> { }
}
