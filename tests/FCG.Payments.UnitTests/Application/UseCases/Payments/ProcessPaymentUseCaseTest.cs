using FCG.Payments.Application.UseCases.Payments.ProcessPayment;
using FCG.Payments.CommomTestUtilities.Builders;
using FCG.Payments.CommomTestUtilities.Builders.Payments.Repositories;
using FCG.Payments.CommomTestUtilities.Builders.Wallets;
using FCG.Payments.CommomTestUtilities.Builders.Wallets.Repositories;
using FCG.Payments.Domain.Abstractions;
using FCG.Payments.Domain.Exceptions;
using FCG.Payments.Domain.Payments;
using FCG.Payments.Domain.Wallets;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace FCG.Payments.UnitTests.Application.UseCases.Payments
{
    public class ProcessPaymentUseCaseTest
    {
        private readonly IReadOnlyWalletRepository _readOnlyWalletRepository;
        private readonly IWriteOnlyPaymentRepository _writeOnlyPaymentRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ProcessPaymentUseCase> _logger;
        private readonly IProcessPaymentUseCase _sut;

        public ProcessPaymentUseCaseTest()
        {
            _readOnlyWalletRepository = ReadOnlyWalletRepositoryBuilder.Build();
            _writeOnlyPaymentRepository = WriteOnlyPaymentRepositoryBuilder.Build();
            _unitOfWork = UnitOfWorkBuilder.Build();
            _logger = new Mock<ILogger<ProcessPaymentUseCase>>().Object;
            _sut = new ProcessPaymentUseCase(_readOnlyWalletRepository, _writeOnlyPaymentRepository, _logger, _unitOfWork);
        }

        [Fact]
        public async Task Given_Valid_Request_With_Sufficient_Balance_When_ProcessPaymentIsCalled_Then_Should_Approve_Payment()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            var amount = 100m;
            var request = new ProcessPaymentRequest(userId, gameId, amount);

            var wallet = new WalletBuilder().BuildWithUserId(userId);
            ReadOnlyWalletRepositoryBuilder.SetupGetByUserIdAsync(userId, wallet);

            // Act
            var response = await _sut.Handle(request, CancellationToken.None);

            // Assert
            response.Should().NotBeNull();
            response.PaymentId.Should().NotBeEmpty();
            response.UserId.Should().Be(userId);
            response.GameId.Should().Be(gameId);
            response.Amount.Should().Be(amount);
            response.Status.Should().Be(PaymentStatus.Approved);
        }

        [Fact]
        public async Task Given_Valid_Request_With_Insufficient_Balance_When_ProcessPaymentIsCalled_Then_Should_Reject_Payment()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            var amount = 5000m;
            var request = new ProcessPaymentRequest(userId, gameId, amount);

            var wallet = new WalletBuilder().BuildWithUserId(userId);
            ReadOnlyWalletRepositoryBuilder.SetupGetByUserIdAsync(userId, wallet);

            // Act
            var response = await _sut.Handle(request, CancellationToken.None);

            // Assert
            response.Should().NotBeNull();
            response.PaymentId.Should().NotBeEmpty();
            response.UserId.Should().Be(userId);
            response.GameId.Should().Be(gameId);
            response.Amount.Should().Be(amount);
            response.Status.Should().Be(PaymentStatus.Rejected);
        }

        [Fact]
        public async Task Given_NonExistent_Wallet_When_ProcessPaymentIsCalled_Then_Should_Throw_NotFoundException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            var amount = 100m;
            var request = new ProcessPaymentRequest(userId, gameId, amount);

            ReadOnlyWalletRepositoryBuilder.SetupGetByUserIdAsync(userId, null);

            // Act
            var act = async () => await _sut.Handle(request, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>().WithMessage($"Wallet not found for UserId: {userId}");
        }
    }
}
