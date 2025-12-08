using FCG.Payments.Domain.Abstractions;
using FCG.Payments.Domain.Exceptions;
using FCG.Payments.Domain.Wallets;
using Microsoft.Extensions.Logging;

namespace FCG.Payments.Application.UseCases.Wallets.DepositBalance
{
    public sealed class DepositBalanceUseCase : IDepositBalanceUseCase
    {
        private readonly IReadOnlyWalletRepository _readOnlyWalletRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DepositBalanceUseCase> _logger;

        public DepositBalanceUseCase(
            IReadOnlyWalletRepository readOnlyWalletRepository,
            IUnitOfWork unitOfWork,
            ILogger<DepositBalanceUseCase> logger)
        {
            _readOnlyWalletRepository = readOnlyWalletRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<DepositBalanceResponse> Handle(DepositBalanceRequest request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling DepositBalanceRequest for WalletId: {WalletId}, Amount: {Amount}", request.Id, request.Amount);

            var wallet = await _readOnlyWalletRepository.GetByIdAsync(request.Id, cancellationToken);

            if (wallet is null)
            {
                _logger.LogWarning("Wallet not found: {WalletId}", request.Id);

                throw new NotFoundException($"Wallet with Id {request.Id} not found");
            }

            wallet.AddBalance(request.Amount);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var updatedWallet = await _readOnlyWalletRepository.GetByIdAsync(request.Id, cancellationToken);

            _logger.LogInformation("Deposit successful for WalletId: {WalletId}, New Balance: {Balance}", request.Id, updatedWallet!.Balance);

            return new DepositBalanceResponse(updatedWallet.Balance);
        }
    }
}
