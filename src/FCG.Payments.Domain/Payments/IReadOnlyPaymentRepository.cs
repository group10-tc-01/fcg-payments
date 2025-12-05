namespace FCG.Payments.Domain.Payments
{
    public interface IReadOnlyPaymentRepository
    {
        Task<Payment?> GetByIdAsync(Guid paymentId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Payment>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
