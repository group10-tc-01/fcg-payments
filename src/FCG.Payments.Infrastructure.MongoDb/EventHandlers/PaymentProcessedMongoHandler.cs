using FCG.Payments.Domain.Payments.Events;
using FCG.Payments.Domain.Payments.Reports;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FCG.Payments.Infrastructure.MongoDb.EventHandlers
{
    public sealed class PaymentProcessedMongoHandler : INotificationHandler<PaymentProcessedEvent>
    {
        private readonly IPaymentReportRepository _paymentReportRepository;
        private readonly ILogger<PaymentProcessedMongoHandler> _logger;

        public PaymentProcessedMongoHandler(
            IPaymentReportRepository paymentReportRepository,
            ILogger<PaymentProcessedMongoHandler> logger)
        {
            _paymentReportRepository = paymentReportRepository;
            _logger = logger;
        }

        public async Task Handle(PaymentProcessedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Persisting payment report for PaymentId {PaymentId}", notification.PaymentId);

            var report = new PaymentReport(
                notification.PaymentId,
                notification.UserId,
                notification.GameId,
                notification.Amount,
                notification.Status,
                notification.ProcessedAt);

            try
            {
                await _paymentReportRepository.InsertAsync(report, cancellationToken);

                _logger.LogInformation("Payment report persisted for PaymentId {PaymentId}", notification.PaymentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to persist payment report for PaymentId {PaymentId}", notification.PaymentId);

                throw;
            }
        }
    }
}
