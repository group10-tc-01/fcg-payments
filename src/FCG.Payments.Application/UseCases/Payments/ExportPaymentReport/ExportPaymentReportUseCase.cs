using FCG.Payments.Application.Abstractions.Reports;
using FCG.Payments.Application.UseCases.Payments.GetPaymentReport;
using FCG.Payments.Domain.Payments;
using FCG.Payments.Domain.Payments.Reports;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FCG.Payments.Application.UseCases.Payments.ExportPaymentReport
{
    public sealed class ExportPaymentReportUseCase : IExportPaymentReportUseCase
    {
        private readonly IPaymentReportRepository _paymentReportRepository;
        private readonly IPaymentReportPdfGenerator _paymentReportPdfGenerator;
        private readonly PaymentReportSettings _settings;
        private readonly ILogger<ExportPaymentReportUseCase> _logger;

        public ExportPaymentReportUseCase(
            IPaymentReportRepository paymentReportRepository,
            IPaymentReportPdfGenerator paymentReportPdfGenerator,
            IOptions<PaymentReportSettings> settings,
            ILogger<ExportPaymentReportUseCase> logger)
        {
            _paymentReportRepository = paymentReportRepository;
            _paymentReportPdfGenerator = paymentReportPdfGenerator;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<ExportPaymentReportResponse> Handle(ExportPaymentReportRequest request, CancellationToken cancellationToken)
        {
            var filter = new PaymentReportFilter(
                ToPaymentStatus(request.Status),
                request.DateFrom,
                request.DateTo,
                request.UserId,
                request.GameId);
            var maxRecords = _settings.GetEffectivePdfExportMaxRecords();

            _logger.LogInformation(
                "Handling ExportPaymentReportRequest - Status: {Status}, DateFrom: {DateFrom}, DateTo: {DateTo}, UserId: {UserId}, GameId: {GameId}, MaxRecords: {MaxRecords}",
                request.Status,
                request.DateFrom,
                request.DateTo,
                request.UserId,
                request.GameId,
                maxRecords);

            var summary = await _paymentReportRepository.GetSummaryAsync(filter, cancellationToken);

            var reports = await _paymentReportRepository.GetAsync(filter, maxRecords, cancellationToken);

            var items = reports
                .Select(report => new GetPaymentReportItemResponse(
                    report.PaymentId,
                    report.UserId,
                    report.GameId,
                    report.Amount,
                    report.Status,
                    report.ProcessedAt))
                .ToList()
                .AsReadOnly();

            var summaryResponse = new GetPaymentReportSummaryResponse(
                summary.TotalPayments,
                summary.TotalApproved,
                summary.TotalRejected,
                summary.ApprovedAmount,
                summary.RejectedAmount);

            var generatedAt = DateTime.UtcNow;
            var pdfData = new PaymentReportPdfData(items, summaryResponse, filter, generatedAt, summary.TotalPayments);
            var pdf = _paymentReportPdfGenerator.Generate(pdfData);
            var fileName = $"payment-reports-{generatedAt:yyyyMMdd-HHmmss}.pdf";

            return new ExportPaymentReportResponse(pdf, "application/pdf", fileName);
        }

        private static PaymentStatus? ToPaymentStatus(PaymentReportStatusFilter? filter) => filter switch
        {
            PaymentReportStatusFilter.Approved => PaymentStatus.Approved,
            PaymentReportStatusFilter.Rejected => PaymentStatus.Rejected,
            _ => null
        };
    }
}
