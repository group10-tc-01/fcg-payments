using FCG.Payments.Application.Abstractions.Messaging;
using FCG.Payments.Domain.Abstractions;
using FCG.Payments.Domain.Exceptions;
using FCG.Payments.Domain.Wallets;

namespace FCG.Payments.Application.UseCases.Wallets.Deposit
{
    public sealed class DepositCommandHandler : ICommandHandler<DepositCommand, DepositResponse>
    {
        private readonly IReadOnlyWalletRepository _readOnlyWalletRepository;
        private readonly IWriteOnlyWalletRepository _writeOnlyWalletRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DepositCommandHandler(
            IReadOnlyWalletRepository readOnlyWalletRepository,
            IWriteOnlyWalletRepository writeOnlyWalletRepository,
            IUnitOfWork unitOfWork)
        {
            _readOnlyWalletRepository = readOnlyWalletRepository;
            _writeOnlyWalletRepository = writeOnlyWalletRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<DepositResponse> Handle(DepositCommand request, CancellationToken cancellationToken)
        {
            var wallet = await _readOnlyWalletRepository.GetByIdAsync(request.WalletId, cancellationToken);

            if (wallet is null)
                throw new DomainException("Wallet not found");

            await _writeOnlyWalletRepository.AddDepositAsync(request.WalletId, request.Amount);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var updatedWallet = await _readOnlyWalletRepository.GetByIdAsync(request.WalletId, cancellationToken);

            return new DepositResponse(updatedWallet!.Id, updatedWallet.Balance.Value);
        }
    }
}
