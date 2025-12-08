using FCG.Payments.Application.Abstractions.Pagination;
using FCG.Payments.Application.UseCases.Payments.GetPaymentHistory;
using FCG.Payments.Domain.Payments;
using FCG.Payments.IntegratedTests.Configurations;
using FCG.Payments.WebApi.Models;
using FluentAssertions;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FCG.Payments.IntegratedTests.Controllers
{
    public class PaymentsControllerTest : FcgFixture
    {
        private const string BaseUrl = "/api/v1/payments";

        public PaymentsControllerTest(CustomWebApplicationFactory factory) : base(factory) { }

        [Fact]
        public async Task Given_ValidRequest_When_GetPaymentHistoryIsCalled_ShouldReturnOk()
        {
            // Arrange
            var url = $"{BaseUrl}/history?pageNumber=1&pageSize=10";
            var adminToken = GenerateToken(Guid.NewGuid(), "Admin");

            // Act
            var result = await DoAuthenticatedGet(url, adminToken);
            var responseContent = await result.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<PagedListResponse<GetPaymentHistoryResponse>>>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
            });

            // Assert
            result.StatusCode.Should().Be(HttpStatusCode.OK);
            apiResponse.Should().NotBeNull();
            apiResponse!.Data.Should().NotBeNull();
            apiResponse.Data.Items.Should().NotBeNull();
            apiResponse.Data.PageSize.Should().Be(10);
        }

        [Fact]
        public async Task Given_ValidRequest_When_GetPaymentHistoryIsCalled_ShouldReturnPayments()
        {
            // Arrange
            var url = $"{BaseUrl}/history?pageNumber=1&pageSize=10";
            var adminToken = GenerateToken(Guid.NewGuid(), "Admin");

            // Act
            var result = await DoAuthenticatedGet(url, adminToken);
            var responseContent = await result.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<PagedListResponse<GetPaymentHistoryResponse>>>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
            });

            // Assert
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            if (apiResponse!.Data.Items.Any())
            {
                var payment = apiResponse.Data.Items.First();
                payment.Id.Should().NotBeEmpty();
                payment.UserId.Should().NotBeEmpty();
                payment.GameId.Should().NotBeEmpty();
                payment.WalletId.Should().NotBeEmpty();
                payment.Amount.Should().BeGreaterThan(0);
                payment.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(5));
            }
        }

        [Fact]
        public async Task Given_StatusFilter_When_GetPaymentHistoryIsCalled_ShouldReturnFilteredResults()
        {
            // Arrange
            var status = PaymentStatus.Pending;
            var url = $"{BaseUrl}/history?pageNumber=1&pageSize=10&status={status}";
            var adminToken = GenerateToken(Guid.NewGuid(), "Admin");

            // Act
            var result = await DoAuthenticatedGet(url, adminToken);
            var responseContent = await result.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<PagedListResponse<GetPaymentHistoryResponse>>>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
            });

            // Assert
            result.StatusCode.Should().Be(HttpStatusCode.OK);
            apiResponse.Should().NotBeNull();
        }

        [Fact]
        public async Task Given_DateRangeFilter_When_GetPaymentHistoryIsCalled_ShouldReturnFilteredResults()
        {
            // Arrange
            var dateFrom = DateTime.UtcNow.AddDays(-1).ToString("yyyy-MM-ddTHH:mm:ss");
            var dateTo = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss");
            var url = $"{BaseUrl}/history?pageNumber=1&pageSize=10&dateFrom={dateFrom}&dateTo={dateTo}";
            var adminToken = GenerateToken(Guid.NewGuid(), "Admin");

            // Act
            var result = await DoAuthenticatedGet(url, adminToken);
            var responseContent = await result.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<PagedListResponse<GetPaymentHistoryResponse>>>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            // Assert
            result.StatusCode.Should().Be(HttpStatusCode.OK);
            apiResponse.Should().NotBeNull();
            apiResponse!.Data.Items.Should().NotBeNull();
        }

        [Fact]
        public async Task Given_UserRole_When_GetPaymentHistoryIsCalled_ShouldReturnForbidden()
        {
            // Arrange
            var url = $"{BaseUrl}/history?pageNumber=1&pageSize=10";
            var userToken = GenerateToken(Guid.NewGuid(), "User");

            // Act
            var result = await DoAuthenticatedGet(url, userToken);

            // Assert
            result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Given_NoAuthentication_When_GetPaymentHistoryIsCalled_ShouldReturnUnauthorized()
        {
            // Arrange
            var url = $"{BaseUrl}/history?pageNumber=1&pageSize=10";

            // Act
            var result = await _httpClient.GetAsync(url);

            // Assert
            result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Given_InvalidPageNumber_When_GetPaymentHistoryIsCalled_ShouldReturnBadRequest()
        {
            // Arrange
            var url = $"{BaseUrl}/history?pageNumber=0&pageSize=10";
            var adminToken = GenerateToken(Guid.NewGuid(), "Admin");

            // Act
            var result = await DoAuthenticatedGet(url, adminToken);

            // Assert
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Given_InvalidPageSize_When_GetPaymentHistoryIsCalled_ShouldReturnBadRequest()
        {
            // Arrange
            var url = $"{BaseUrl}/history?pageNumber=1&pageSize=0";
            var adminToken = GenerateToken(Guid.NewGuid(), "Admin");

            // Act
            var result = await DoAuthenticatedGet(url, adminToken);

            // Assert
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Given_PageSizeGreaterThan50_When_GetPaymentHistoryIsCalled_ShouldReturnBadRequest()
        {
            // Arrange
            var url = $"{BaseUrl}/history?pageNumber=1&pageSize=51";
            var adminToken = GenerateToken(Guid.NewGuid(), "Admin");

            // Act
            var result = await DoAuthenticatedGet(url, adminToken);

            // Assert
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}
