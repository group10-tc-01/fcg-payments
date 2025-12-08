using FCG.Payments.Domain.Payments;
using Moq;

namespace FCG.Payments.CommomTestUtilities.Builders.Payments.Repositories
{
    public static class ReadOnlyPaymentRepositoryBuilder
    {
        public static readonly Mock<IReadOnlyPaymentRepository> _mock = new Mock<IReadOnlyPaymentRepository>();

        public static IReadOnlyPaymentRepository Build() => _mock.Object;

        public static void SetupGetByIdAsync(Guid paymentId, Payment? payment)
        {
            _mock.Setup(repo => repo.GetByIdAsync(paymentId, It.IsAny<CancellationToken>())).ReturnsAsync(payment);
        }

        public static void SetupGetByUserIdAsync(Guid userId, IEnumerable<Payment> payments)
        {
            _mock.Setup(repo => repo.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(payments);
        }
    }
}
