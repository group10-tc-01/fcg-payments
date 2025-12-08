using FCG.Payments.Domain.Abstractions;
using FCG.Payments.Domain.Exceptions;
using FCG.Payments.Domain.Wallets;
using Microsoft.Extensions.Logging;

namespace FCG.Payments.Application.UseCases.Wallets.CreateWallet
{
    public sealed class CreateWalletUseCase : ICreateWalletUseCase
    {
        private readonly IWriteOnlyWalletRepository _writeOnlyWalletRepository;
        private readonly IReadOnlyWalletRepository _readOnlyWalletRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CreateWalletUseCase> _logger;

        public CreateWalletUseCase(
            IWriteOnlyWalletRepository writeOnlyWalletRepository,
            IUnitOfWork unitOfWork,
            ILogger<CreateWalletUseCase> logger,
            IReadOnlyWalletRepository readOnlyWalletRepository)
        {
            _writeOnlyWalletRepository = writeOnlyWalletRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _readOnlyWalletRepository = readOnlyWalletRepository;
        }

        public async Task<CreateWalletResponse> Handle(CreateWalletRequest request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating wallet for user {UserId}", request.UserId);

            var walletExists = await _readOnlyWalletRepository.GetByUserIdAsync(request.UserId, cancellationToken);

            if (walletExists is not null)
            {
                _logger.LogWarning("Wallet already exists for user {UserId}", request.UserId);

                throw new ConflictException($"Wallet already exists for user {request.UserId}");
            }

            var wallet = Wallet.CreateWallet(request.UserId);

            await _writeOnlyWalletRepository.AddAsync(wallet, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Wallet {WalletId} created for user {UserId}", wallet.Id, request.UserId);

            return new CreateWalletResponse(wallet.Id);
        }
    }
}
