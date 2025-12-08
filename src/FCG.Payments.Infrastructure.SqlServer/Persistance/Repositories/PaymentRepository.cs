using FCG.Payments.Domain.Payments;
using Microsoft.EntityFrameworkCore;

namespace FCG.Payments.Infrastructure.SqlServer.Persistance.Repositories
{
    public sealed class PaymentRepository : IReadOnlyPaymentRepository, IWriteOnlyPaymentRepository
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

        public async Task<Payment?> GetByIdAsync(Guid paymentId, CancellationToken cancellationToken = default)
        {
            return await _fcgPaymentDbContext.Payment
                .FirstOrDefaultAsync(p => p.Id == paymentId, cancellationToken);
        }

        public async Task<IEnumerable<Payment>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _fcgPaymentDbContext.Payment.Where(p => p.UserId == userId).ToListAsync(cancellationToken);
        }

        public async Task<(IEnumerable<Payment> Payments, int TotalCount)> GetPaymentHistoryAsync(
            int pageNumber,
            int pageSize,
            PaymentStatus? status,
            DateTime? dateFrom,
            DateTime? dateTo,
            CancellationToken cancellationToken = default)
        {
            var query = _fcgPaymentDbContext.Payment.AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(p => p.Status == status.Value);
            }

            if (dateFrom.HasValue)
            {
                query = query.Where(p => p.CreatedAt >= dateFrom.Value);
            }

            if (dateTo.HasValue)
            {
                query = query.Where(p => p.CreatedAt <= dateTo.Value);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var payments = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (payments, totalCount);
        }
    }
}
