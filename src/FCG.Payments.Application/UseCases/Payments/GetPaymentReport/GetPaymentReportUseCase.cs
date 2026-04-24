using FCG.Payments.Domain.Payments.Reports;
using Microsoft.Extensions.Logging;

namespace FCG.Payments.Application.UseCases.Payments.GetPaymentReport
{
    public sealed class GetPaymentReportUseCase : IGetPaymentReportUseCase
    {
        private readonly IPaymentReportRepository _paymentReportRepository;
        private readonly ILogger<GetPaymentReportUseCase> _logger;

        public GetPaymentReportUseCase(
            IPaymentReportRepository paymentReportRepository,
            ILogger<GetPaymentReportUseCase> logger)
        {
            _paymentReportRepository = paymentReportRepository;
            _logger = logger;
        }

        public async Task<GetPaymentReportResponse> Handle(GetPaymentReportRequest request, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Handling GetPaymentReportRequest - PageNumber: {PageNumber}, PageSize: {PageSize}",
                request.PageNumber,
                request.PageSize);

            var (reports, totalCount) = await _paymentReportRepository.GetPagedAsync(
                request.PageNumber,
                request.PageSize,
                cancellationToken);

            var summary = await _paymentReportRepository.GetSummaryAsync(cancellationToken);

            var items = reports.Select(report => new GetPaymentReportItemResponse(
                report.PaymentId,
                report.UserId,
                report.GameId,
                report.Amount,
                report.Status,
                report.ProcessedAt));

            var summaryResponse = new GetPaymentReportSummaryResponse(
                summary.TotalPayments,
                summary.TotalApproved,
                summary.TotalRejected,
                summary.ApprovedAmount,
                summary.RejectedAmount);

            _logger.LogInformation(
                "Payment report retrieved successfully - TotalCount: {TotalCount}, CurrentPage: {CurrentPage}",
                totalCount,
                request.PageNumber);

            return new GetPaymentReportResponse(
                items,
                totalCount,
                request.PageNumber,
                request.PageSize,
                summaryResponse);
        }
    }
}
