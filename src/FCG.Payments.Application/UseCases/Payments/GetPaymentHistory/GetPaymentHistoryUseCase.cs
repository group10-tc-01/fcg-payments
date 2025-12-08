using FCG.Payments.Application.Abstractions.Pagination;
using FCG.Payments.Domain.Payments;
using Microsoft.Extensions.Logging;

namespace FCG.Payments.Application.UseCases.Payments.GetPaymentHistory
{
    public sealed class GetPaymentHistoryUseCase : IGetPaymentHistoryUseCase
    {
        private readonly IReadOnlyPaymentRepository _readOnlyPaymentRepository;
        private readonly ILogger<GetPaymentHistoryUseCase> _logger;

        public GetPaymentHistoryUseCase(
            IReadOnlyPaymentRepository readOnlyPaymentRepository,
            ILogger<GetPaymentHistoryUseCase> logger)
        {
            _readOnlyPaymentRepository = readOnlyPaymentRepository;
            _logger = logger;
        }

        public async Task<PagedListResponse<GetPaymentHistoryResponse>> Handle(
            GetPaymentHistoryRequest request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Handling GetPaymentHistoryRequest - PageNumber: {PageNumber}, PageSize: {PageSize}, Status: {Status}, DateFrom: {DateFrom}, DateTo: {DateTo}",
                request.PageNumber,
                request.PageSize,
                request.Status,
                request.DateFrom,
                request.DateTo);

            var (payments, totalCount) = await _readOnlyPaymentRepository.GetPaymentHistoryAsync(
                request.PageNumber,
                request.PageSize,
                request.Status,
                request.DateFrom,
                request.DateTo,
                cancellationToken);

            var response = payments.Select(p => new GetPaymentHistoryResponse(
                p.Id,
                p.UserId,
                p.GameId,
                p.WalletId,
                p.Amount,
                p.Status,
                p.FailureReason,
                p.ProcessedAt,
                p.CreatedAt)).ToList();

            _logger.LogInformation(
                "Payment history retrieved successfully - TotalCount: {TotalCount}, CurrentPage: {CurrentPage}",
                totalCount,
                request.PageNumber);

            return new PagedListResponse<GetPaymentHistoryResponse>(
                response,
                totalCount,
                request.PageNumber,
                request.PageSize);
        }
    }
}
