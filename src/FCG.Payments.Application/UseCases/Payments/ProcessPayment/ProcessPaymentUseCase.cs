
using FCG.Payments.Domain.Abstractions;
using FCG.Payments.Domain.Exceptions;
using FCG.Payments.Domain.Payments;
using FCG.Payments.Domain.Wallets;
using Microsoft.Extensions.Logging;

namespace FCG.Payments.Application.UseCases.Payments.ProcessPayment
{
    public sealed class ProcessPaymentUseCase : IProcessPaymentUseCase
    {
        private readonly IReadOnlyWalletRepository _readOnlyWalletRepository;
        private readonly IWriteOnlyPaymentRepository _writeOnlyPaymentRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ProcessPaymentUseCase> _logger;

        public ProcessPaymentUseCase(
            IReadOnlyWalletRepository readOnlyWalletRepository,
            IWriteOnlyPaymentRepository writeOnlyPaymentRepository,
            ILogger<ProcessPaymentUseCase> logger,
            IUnitOfWork unitOfWork)
        {
            _readOnlyWalletRepository = readOnlyWalletRepository;
            _writeOnlyPaymentRepository = writeOnlyPaymentRepository;
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<ProcessPaymentResponse> Handle(ProcessPaymentRequest request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Processing payment for UserId: {UserId}, GameId: {GameId}, Amount: {Amount}", request.UserId, request.GameId, request.Amount);

            var wallet = await _readOnlyWalletRepository.GetByUserIdAsync(request.UserId, cancellationToken);

            if (wallet == null)
            {
                _logger.LogWarning("Wallet not found for UserId: {UserId}", request.UserId);

                throw new NotFoundException($"Wallet not found for UserId: {request.UserId}");
            }

            var payment = Payment.CreatePayment(request.UserId, request.GameId, wallet.Id, request.Amount);

            _logger.LogInformation("Wallet balance before debit: {Balance}", wallet.Balance);

            var debitSuccessful = wallet.TryDebit(request.Amount);

            _logger.LogInformation("Debit successful: {DebitSuccessful}, Wallet balance after debit: {Balance}", debitSuccessful, wallet.Balance);

            if (debitSuccessful)
            {
                _logger.LogInformation("Payment approved for UserId: {UserId}, PaymentId: {PaymentId}", request.UserId, payment.Id);

                payment.Approve();
            }

            if (debitSuccessful is false)
            {
                _logger.LogWarning("Payment rejected due to insufficient balance for UserId: {UserId}, PaymentId: {PaymentId}", request.UserId, payment.Id);

                payment.Reject("Insufficient balance");
            }

            await _writeOnlyPaymentRepository.AddAsync(payment, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new ProcessPaymentResponse(payment.Id, payment.UserId, payment.GameId, payment.Amount, payment.Status);
        }
    }
}
