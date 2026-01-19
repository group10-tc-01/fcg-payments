using Bogus;
using FCG.Payments.Domain.Payments;

namespace FCG.Payments.CommomTestUtilities.Builders.Payments
{
    public class PaymentBuilder
    {
        public Payment Build()
        {
            return new Faker<Payment>()
                .CustomInstantiator(f => Payment.CreatePayment(
                    f.Internet.Email(),
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    f.Finance.Amount(1, 1000)))
                .Generate();
        }

        public Payment BuildWithParameters(string userEmail, Guid correlationId, Guid userId, Guid gameId, Guid walletId, decimal amount)
        {
            return Payment.CreatePayment(userEmail, correlationId, userId, gameId, walletId, amount);
        }
    }
}
