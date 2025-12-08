using FCG.Payments.Domain.Payments;

namespace FCG.Payments.Infrastructure.SqlServer.Persistance.Repositories
{
    public sealed class PaymentRepository : IWriteOnlyPaymentRepository
    {

        private readonly FcgPaymentDbContext _fcgPaymentDbContext;

        public PaymentRepository(FcgPaymentDbContext dbContext)
        {
            _fcgPaymentDbContext = dbContext;
        }


        public async Task AddAsync(Payment payment, CancellationToken cancellationToken = default)
        {
            await _fcgPaymentDbContext.Payment.AddAsync(payment, cancellationToken);
        }
    }
}
