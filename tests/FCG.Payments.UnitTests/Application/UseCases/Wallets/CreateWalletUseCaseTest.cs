using FCG.Payments.Application.UseCases.Wallets.CreateWallet;
using FCG.Payments.CommomTestUtilities.Builders;
using FCG.Payments.CommomTestUtilities.Builders.Wallets;
using FCG.Payments.CommomTestUtilities.Builders.Wallets.Repositories;
using FCG.Payments.Domain.Abstractions;
using FCG.Payments.Domain.Exceptions;
using FCG.Payments.Domain.Wallets;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace FCG.Payments.UnitTests.Application.UseCases.Wallets
{
    public class CreateWalletUseCaseTest
    {
        private readonly IWriteOnlyWalletRepository _writeOnlyWalletRepository;
        private readonly IReadOnlyWalletRepository _readOnlyWalletRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CreateWalletUseCase> _logger;
        private readonly ICreateWalletUseCase _sut;

        public CreateWalletUseCaseTest()
        {
            _writeOnlyWalletRepository = WriteOnlyWalletRepositoryBuilder.Build();
            _readOnlyWalletRepository = ReadOnlyWalletRepositoryBuilder.Build();
            _unitOfWork = UnitOfWorkBuilder.Build();
            _logger = new Mock<ILogger<CreateWalletUseCase>>().Object;
            _sut = new CreateWalletUseCase(_writeOnlyWalletRepository, _unitOfWork, _logger, _readOnlyWalletRepository);
        }


        [Fact]
        public async Task Given_Valid_Request_When_CreateWalletIsCalled_Then_Should_Return_Wallet()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = new CreateWalletRequest(userId);

            // Act
            var response = await _sut.Handle(request, CancellationToken.None);

            // Assert
            response.Should().NotBeNull();
            response.WalletId.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Given_Existing_Wallet_ForUserId_When_CreateWalletIsCalled_Then_ShouldThrowConflictException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = new CreateWalletRequest(userId);
            var wallet = new WalletBuilder().Build();
            ReadOnlyWalletRepositoryBuilder.SetupGetByUserIdAsync(userId, wallet);

            // Act
            var act = async () => await _sut.Handle(request, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ConflictException>().WithMessage($"Wallet already exists for user {request.UserId}");
        }
    }
}
