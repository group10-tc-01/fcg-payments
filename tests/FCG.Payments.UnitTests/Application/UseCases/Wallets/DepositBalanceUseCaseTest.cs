using FCG.Payments.Application.UseCases.Wallets.DepositBalance;
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
    public class DepositBalanceUseCaseTest
    {
        private readonly IReadOnlyWalletRepository _readOnlyWalletRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DepositBalanceUseCase> _logger;
        private readonly IDepositBalanceUseCase _sut;

        public DepositBalanceUseCaseTest()
        {
            _readOnlyWalletRepository = ReadOnlyWalletRepositoryBuilder.Build();
            _unitOfWork = UnitOfWorkBuilder.Build();
            _logger = new Mock<ILogger<DepositBalanceUseCase>>().Object;
            _sut = new DepositBalanceUseCase(_readOnlyWalletRepository, _unitOfWork, _logger);
        }

        [Fact]
        public async Task Given_Valid_Request_When_DepositBalanceIsCalled_Then_Should_Return_UpdatedBalance()
        {
            // Arrange
            var walletId = Guid.NewGuid();
            var depositAmount = 500m;
            var wallet = new WalletBuilder().Build();
            var initialBalance = wallet.Balance;

            ReadOnlyWalletRepositoryBuilder._mock
                .Setup(repo => repo.GetByIdAsync(walletId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(wallet);

            var request = new DepositBalanceRequest(walletId, depositAmount);

            // Act
            var response = await _sut.Handle(request, CancellationToken.None);

            // Assert
            response.Should().NotBeNull();
            response.Balance.Should().Be(initialBalance + depositAmount);
        }

        [Fact]
        public async Task Given_Invalid_WalletId_When_DepositBalanceIsCalled_Then_Should_Throw_NotFoundException()
        {
            // Arrange
            var walletId = Guid.NewGuid();
            var depositAmount = 500m;

            ReadOnlyWalletRepositoryBuilder._mock
                .Setup(repo => repo.GetByIdAsync(walletId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Wallet?)null);

            var request = new DepositBalanceRequest(walletId, depositAmount);

            // Act
            var act = async () => await _sut.Handle(request, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage($"Wallet with Id {walletId} not found");
        }

        [Fact]
        public void DepositBalanceRequest_Should_Have_All_Properties()
        {
            // Arrange
            var walletId = Guid.NewGuid();
            var amount = 250m;

            // Act
            var request = new DepositBalanceRequest(walletId, amount);

            // Assert
            request.Id.Should().Be(walletId);
            request.Amount.Should().Be(amount);
        }

        [Fact]
        public void DepositBalanceResponse_Should_Have_All_Properties()
        {
            // Arrange
            var balance = 1500m;

            // Act
            var response = new DepositBalanceResponse(balance);

            // Assert
            response.Balance.Should().Be(balance);
        }
    }
}
