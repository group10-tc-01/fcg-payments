using FCG.Payments.Domain.Abstractions;
using Moq;

namespace FCG.Payments.CommomTestUtilities.Builders
{
    public static class UnitOfWorkBuilder
    {
        private static readonly Mock<IUnitOfWork> _mock = new Mock<IUnitOfWork>();

        public static IUnitOfWork Build() => _mock.Object;

        public static void SetupSaveChangesAsync(int returnValue = 1)
        {
            _mock.Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(returnValue);
        }
    }
}
