using FCG.Payments.Domain.Wallets;
using Moq;

namespace FCG.Payments.CommomTestUtilities.Builders.Wallets.Repositories
{
    public static class WriteOnlyWalletRepositoryBuilder
    {
        public static readonly Mock<IWriteOnlyWalletRepository> _mock = new Mock<IWriteOnlyWalletRepository>();

        public static IWriteOnlyWalletRepository Build() => _mock.Object;
    }
}
