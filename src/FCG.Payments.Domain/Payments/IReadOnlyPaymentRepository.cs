namespace FCG.Payments.Domain.Payments
{
    public interface IReadOnlyPaymentRepository
    {
        Task<Payment?> GetByIdAsync(Guid paymentId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Payment>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<(IEnumerable<Payment> Payments, int TotalCount)> GetPaymentHistoryAsync(
            int pageNumber,
            int pageSize,
            PaymentStatus? status,
            DateTime? dateFrom,
            DateTime? dateTo,
            CancellationToken cancellationToken = default);
    }
}
