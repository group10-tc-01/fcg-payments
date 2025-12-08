using FCG.Payments.Application.UseCases.Wallets.GetWalletBalance;
using FCG.Payments.CommomTestUtilities.Builders.Wallets;
using FCG.Payments.CommomTestUtilities.Builders.Wallets.Repositories;
using FCG.Payments.Domain.Exceptions;
using FCG.Payments.Domain.Wallets;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace FCG.Payments.UnitTests.Application.UseCases.Wallets
{
    public class GetWalletBalanceUseCaseTest
    {
        private readonly IReadOnlyWalletRepository _readOnlyWalletRepository;
        private readonly ILogger<GetWalletBalanceUseCase> _logger;
        private readonly IGetWalletBalanceUseCase _sut;

        public GetWalletBalanceUseCaseTest()
        {
            _readOnlyWalletRepository = ReadOnlyWalletRepositoryBuilder.Build();
            _logger = new Mock<ILogger<GetWalletBalanceUseCase>>().Object;
            _sut = new GetWalletBalanceUseCase(_readOnlyWalletRepository, _logger);
        }

        [Fact]
        public async Task Given_Valid_WalletId_When_GetWalletBalanceIsCalled_Then_Should_Return_Balance()
        {
            // Arrange
            var walletId = Guid.NewGuid();
            var wallet = new WalletBuilder().Build();
            var expectedBalance = wallet.Balance;

            ReadOnlyWalletRepositoryBuilder._mock
                .Setup(repo => repo.GetByIdAsync(walletId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(wallet);

            var request = new GetWalletBalanceRequest(walletId);

            // Act
            var response = await _sut.Handle(request, CancellationToken.None);

            // Assert
            response.Should().NotBeNull();
            response.Balance.Should().Be(expectedBalance);
        }

        [Fact]
        public async Task Given_Invalid_WalletId_When_GetWalletBalanceIsCalled_Then_Should_Throw_NotFoundException()
        {
            // Arrange
            var walletId = Guid.NewGuid();

            ReadOnlyWalletRepositoryBuilder._mock
                .Setup(repo => repo.GetByIdAsync(walletId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Wallet?)null);

            var request = new GetWalletBalanceRequest(walletId);

            // Act
            var act = async () => await _sut.Handle(request, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage($"Wallet with Id {walletId} not found");
        }

        [Fact]
        public void GetWalletBalanceRequest_Should_Have_All_Properties()
        {
            // Arrange
            var walletId = Guid.NewGuid();

            // Act
            var request = new GetWalletBalanceRequest(walletId);

            // Assert
            request.Id.Should().Be(walletId);
        }

        [Fact]
        public void GetWalletBalanceResponse_Should_Have_All_Properties()
        {
            // Arrange
            var balance = 1000m;

            // Act
            var response = new GetWalletBalanceResponse(balance);

            // Assert
            response.Balance.Should().Be(balance);
        }
    }
}
