using FCG.Payments.Application.UseCases.Wallets.DepositBalance;
using FCG.Payments.Application.UseCases.Wallets.GetWalletBalance;
using FCG.Payments.IntegratedTests.Configurations;
using FCG.Payments.WebApi.Models;
using FluentAssertions;
using System.Net;
using System.Text.Json;

namespace FCG.Payments.IntegratedTests.Controllers
{
    public class WalletsControllerTest : FcgFixture
    {
        private const string BaseUrl = "/api/wallets";

        public WalletsControllerTest(CustomWebApplicationFactory factory) : base(factory) { }

        [Fact]
        public async Task Given_ValidWalletId_When_GetWalletBalanceIsCalled_ShouldReturnOk()
        {
            // Arrange
            var wallet = Factory.CreatedWallets.First();
            var url = $"{BaseUrl}/{wallet.Id}/balance";
            var userToken = GenerateToken(wallet.UserId, "User");

            // Act
            var result = await DoAuthenticatedGet(url, userToken);
            var responseContent = await result.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<GetWalletBalanceResponse>>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            // Assert
            result.StatusCode.Should().Be(HttpStatusCode.OK);
            apiResponse.Should().NotBeNull();
            apiResponse!.Data.Should().NotBeNull();
        }

        [Fact]
        public async Task Given_InvalidWalletId_When_GetWalletBalanceIsCalled_ShouldReturnNotFound()
        {
            // Arrange
            var invalidWalletId = Guid.NewGuid();
            var url = $"{BaseUrl}/{invalidWalletId}/balance";
            var userToken = GenerateToken(Guid.NewGuid(), "User");

            // Act
            var result = await DoAuthenticatedGet(url, userToken);

            // Assert
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Given_NoAuthentication_When_GetWalletBalanceIsCalled_ShouldReturnUnauthorized()
        {
            // Arrange
            var wallet = Factory.CreatedWallets.First();
            var url = $"{BaseUrl}/{wallet.Id}/balance";

            // Act
            var result = await _httpClient.GetAsync(url);

            // Assert
            result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Given_ValidDepositRequest_When_DepositBalanceIsCalled_ShouldReturnOk()
        {
            // Arrange
            var wallet = Factory.CreatedWallets.First();
            var url = $"{BaseUrl}/{wallet.Id}/deposit";
            var adminToken = GenerateToken(Guid.NewGuid(), "Admin");
            var request = new DepositBalanceRequestBody(500m);

            // Act
            var result = await DoAuthenticatedPost(url, request, adminToken);
            var responseContent = await result.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<DepositBalanceResponse>>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            // Assert
            result.StatusCode.Should().Be(HttpStatusCode.OK);
            apiResponse.Should().NotBeNull();
            apiResponse!.Data.Should().NotBeNull();
            apiResponse.Data.Balance.Should().BeGreaterThan(wallet.Balance);
        }

        [Fact]
        public async Task Given_InvalidWalletId_When_DepositBalanceIsCalled_ShouldReturnNotFound()
        {
            // Arrange
            var invalidWalletId = Guid.NewGuid();
            var url = $"{BaseUrl}/{invalidWalletId}/deposit";
            var adminToken = GenerateToken(Guid.NewGuid(), "Admin");
            var request = new DepositBalanceRequestBody(500m);

            // Act
            var result = await DoAuthenticatedPost(url, request, adminToken);

            // Assert
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Given_UserRole_When_DepositBalanceIsCalled_ShouldReturnForbidden()
        {
            // Arrange
            var wallet = Factory.CreatedWallets.First();
            var url = $"{BaseUrl}/{wallet.Id}/deposit";
            var userToken = GenerateToken(wallet.UserId, "User");
            var request = new DepositBalanceRequestBody(500m);

            // Act
            var result = await DoAuthenticatedPost(url, request, userToken);

            // Assert
            result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Given_NoAuthentication_When_DepositBalanceIsCalled_ShouldReturnUnauthorized()
        {
            // Arrange
            var wallet = Factory.CreatedWallets.First();
            var url = $"{BaseUrl}/{wallet.Id}/deposit";
            var request = new DepositBalanceRequestBody(500m);

            // Act
            var result = await DoPost(url, request);

            // Assert
            result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Given_InvalidAmount_When_DepositBalanceIsCalled_ShouldReturnBadRequest()
        {
            // Arrange
            var wallet = Factory.CreatedWallets.First();
            var url = $"{BaseUrl}/{wallet.Id}/deposit";
            var adminToken = GenerateToken(Guid.NewGuid(), "Admin");
            var request = new DepositBalanceRequestBody(-100m);

            // Act
            var result = await DoAuthenticatedPost(url, request, adminToken);

            // Assert
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Given_ZeroAmount_When_DepositBalanceIsCalled_ShouldReturnBadRequest()
        {
            // Arrange
            var wallet = Factory.CreatedWallets.First();
            var url = $"{BaseUrl}/{wallet.Id}/deposit";
            var adminToken = GenerateToken(Guid.NewGuid(), "Admin");
            var request = new DepositBalanceRequestBody(0m);

            // Act
            var result = await DoAuthenticatedPost(url, request, adminToken);

            // Assert
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}
