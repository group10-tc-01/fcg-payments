using FCG.Payments.Domain.Wallets;
using FCG.Payments.Infrastructure.SqlServer.Persistance;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace FCG.Payments.Infrastructure.SqlServer.Repositories
{
    [ExcludeFromCodeCoverage]
    public class WalletRepository : IReadOnlyWalletRepository, IWriteOnlyWalletRepository
    {
        private readonly FcgPaymentDbContext _context;

        public WalletRepository(FcgPaymentDbContext context)
        {
            _context = context;
        }

        public async Task<Wallet?> GetByIdAsync(Guid walletId, CancellationToken cancellationToken = default)
        {
            return await _context.Wallet
                .FirstOrDefaultAsync(w => w.Id == walletId, cancellationToken);
        }

        public async Task AddDepositAsync(Guid walletId, decimal amount)
        {
            var wallet = await _context.Wallet.FindAsync(new object[] { walletId }, CancellationToken.None);

            if (wallet is null)
                throw new InvalidOperationException("Wallet not found");

            wallet.AddDeposit(amount);

            _context.Wallet.Update(wallet);
        }
    }
}
