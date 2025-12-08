
using FCG.Payments.Domain.Exceptions;
using FCG.Payments.Domain.Wallets;
using Microsoft.Extensions.Logging;

namespace FCG.Payments.Application.UseCases.Wallets.GetWalletBalance
{
    public sealed class GetWalletBalanceUseCase : IGetWalletBalanceUseCase
    {
        private readonly IReadOnlyWalletRepository _readOnlyWalletRepository;
        private readonly ILogger<GetWalletBalanceUseCase> _logger;

        public GetWalletBalanceUseCase(
            IReadOnlyWalletRepository readOnlyWalletRepository,
            ILogger<GetWalletBalanceUseCase> logger)
        {
            _readOnlyWalletRepository = readOnlyWalletRepository;
            _logger = logger;
        }

        public async Task<GetWalletBalanceResponse> Handle(GetWalletBalanceRequest request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling GetWalletBalanceRequest for WalletId: {WalletId}", request.Id);

            var wallet = await _readOnlyWalletRepository.GetByIdAsync(request.Id, cancellationToken);

            if (wallet is null)
            {
                _logger.LogWarning("Wallet not found: {WalletId}", request.Id);

                throw new NotFoundException($"Wallet with Id {request.Id} not found");
            }

            return new GetWalletBalanceResponse(wallet.Balance);
        }
    }
}
