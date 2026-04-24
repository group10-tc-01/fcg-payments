namespace FCG.Payments.Domain.Payments.Reports
{
    public interface IPaymentReportRepository
    {
        Task InsertAsync(PaymentReport report, CancellationToken cancellationToken = default);

        Task<(IEnumerable<PaymentReport> Reports, int TotalCount)> GetByUserIdAsync(
            Guid userId,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default);

        Task<(IEnumerable<PaymentReport> Reports, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default);

        Task<PaymentReportSummary> GetSummaryAsync(CancellationToken cancellationToken = default);
    }
}
