using FCG.Payments.Domain.Payments;
using Moq;

namespace FCG.Payments.CommomTestUtilities.Builders.Payments.Repositories
{
    public static class WriteOnlyPaymentRepositoryBuilder
    {
        public static readonly Mock<IWriteOnlyPaymentRepository> _mock = new Mock<IWriteOnlyPaymentRepository>();

        public static IWriteOnlyPaymentRepository Build() => _mock.Object;

        public static void SetupAddAsync(Payment payment)
        {
            _mock.Setup(repo => repo.AddAsync(payment, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        }
    }
}
