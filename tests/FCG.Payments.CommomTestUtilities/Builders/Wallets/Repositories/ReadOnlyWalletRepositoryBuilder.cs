using FCG.Payments.Domain.Wallets;
using Moq;

namespace FCG.Payments.CommomTestUtilities.Builders.Wallets.Repositories
{
    public static class ReadOnlyWalletRepositoryBuilder
    {
        public static readonly Mock<IReadOnlyWalletRepository> _mock = new Mock<IReadOnlyWalletRepository>();

        public static IReadOnlyWalletRepository Build() => _mock.Object;

        public static void SetupGetByUserIdAsync(Guid userId, Wallet? wallet)
        {
            _mock.Setup(repo => repo.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(wallet);
        }
    }
}
