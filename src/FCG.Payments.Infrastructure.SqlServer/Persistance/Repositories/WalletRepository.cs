using FCG.Payments.Domain.Wallets;
using Microsoft.EntityFrameworkCore;

namespace FCG.Payments.Infrastructure.SqlServer.Persistance.Repositories
{
    public sealed class WalletRepository : IReadOnlyWalletRepository, IWriteOnlyWalletRepository
    {
        private readonly FcgPaymentDbContext _fcgPaymentDbContext;

        public WalletRepository(FcgPaymentDbContext dbContext)
        {
            _fcgPaymentDbContext = dbContext;
        }

        public async Task AddAsync(Wallet wallet, CancellationToken cancellationToken)
        {
            await _fcgPaymentDbContext.Wallet.AddAsync(wallet, cancellationToken);
        }

        public Task AddDepositAync(Guid walletId, decimal amount)
        {
            throw new NotImplementedException();
        }

        public Task<Wallet?> GetByIdAsync(Guid walletId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<Wallet?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            return await _fcgPaymentDbContext.Wallet.AsNoTracking().FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken);
        }
    }
}
