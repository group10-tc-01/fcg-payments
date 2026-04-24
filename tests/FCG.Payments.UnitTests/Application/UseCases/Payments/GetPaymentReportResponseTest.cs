using FCG.Payments.Application.UseCases.Payments.GetPaymentReport;
using FCG.Payments.Domain.Payments;
using FluentAssertions;

namespace FCG.Payments.UnitTests.Application.UseCases.Payments
{
    public class GetPaymentReportResponseTest
    {
        [Fact]
        public void Given_JsonConstructor_Parameters_When_ResponseIsCreated_Then_Should_Set_Pagination_Properties()
        {
            // Arrange
            var items = new List<GetPaymentReportItemResponse>
            {
                new(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    125m,
                    PaymentStatus.Approved,
                    DateTime.UtcNow)
            }.AsReadOnly();
            var summary = new GetPaymentReportSummaryResponse(3, 2, 1, 225m, 50m);

            // Act
            var response = new GetPaymentReportResponse(
                items,
                currentPage: 2,
                totalPages: 3,
                pageSize: 1,
                totalCount: 3,
                summary);

            // Assert
            response.Items.Should().BeSameAs(items);
            response.CurrentPage.Should().Be(2);
            response.TotalPages.Should().Be(3);
            response.PageSize.Should().Be(1);
            response.TotalCount.Should().Be(3);
            response.Summary.Should().Be(summary);
            response.HasPrevious.Should().BeTrue();
            response.HasNext.Should().BeTrue();
        }

        [Fact]
        public void Given_First_Page_When_ResponseIsCreated_Then_Should_Not_Have_Previous_Page()
        {
            // Arrange
            var summary = new GetPaymentReportSummaryResponse(1, 1, 0, 125m, 0m);

            // Act
            var response = new GetPaymentReportResponse(
                [],
                totalCount: 1,
                currentPage: 1,
                pageSize: 10,
                summary);

            // Assert
            response.HasPrevious.Should().BeFalse();
            response.HasNext.Should().BeFalse();
        }
    }
}
