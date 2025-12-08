using Bogus;
using FCG.Payments.Domain.Wallets;

namespace FCG.Payments.CommomTestUtilities.Builders.Wallets
{
    public class WalletBuilder
    {
        public Wallet Build()
        {
            return new Faker<Wallet>()
                .CustomInstantiator(f => Wallet.CreateWallet(Guid.NewGuid()))
                .Generate();
        }

        public Wallet BuildWithUserId(Guid userId)
        {
            return Wallet.CreateWallet(userId);
        }
    }
}
