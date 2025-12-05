namespace FCG.Payments.Domain.Payments
{
    public interface IWriteOnlyPaymentRepository
    {
        Task AddAsync(Payment payment, CancellationToken cancellationToken = default);
    }
}
