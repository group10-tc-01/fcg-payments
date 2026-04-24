using FCG.Payments.Domain.Payments;
using FCG.Payments.Domain.Payments.Reports;
using FCG.Payments.Infrastructure.MongoDb.Documents;
using FCG.Payments.Infrastructure.MongoDb.Repositories;
using FluentAssertions;
using MongoDB.Driver;
using Moq;

namespace FCG.Payments.UnitTests.Infrastructure.MongoDb.Repositories
{
    public class PaymentReportMongoRepositoryTest
    {
        private const string CollectionName = "payment_reports";
        private readonly Mock<IMongoCollection<PaymentReportDocument>> _collectionMock;
        private readonly PaymentReportMongoRepository _sut;

        public PaymentReportMongoRepositoryTest()
        {
            _collectionMock = new Mock<IMongoCollection<PaymentReportDocument>>();

            var databaseMock = new Mock<IMongoDatabase>();
            databaseMock
                .Setup(database => database.GetCollection<PaymentReportDocument>(
                    CollectionName,
                    It.IsAny<MongoCollectionSettings?>()))
                .Returns(_collectionMock.Object);

            _sut = new PaymentReportMongoRepository(databaseMock.Object);
        }

        [Fact]
        public async Task Given_PaymentReport_When_InsertAsyncIsCalled_Then_Should_Insert_Mapped_Document()
        {
            // Arrange
            var cancellationToken = new CancellationTokenSource().Token;
            var report = CreateReport(PaymentStatus.Approved, 125m, DateTime.UtcNow);
            PaymentReportDocument? insertedDocument = null;

            _collectionMock
                .Setup(collection => collection.InsertOneAsync(
                    It.IsAny<PaymentReportDocument>(),
                    It.IsAny<InsertOneOptions?>(),
                    It.IsAny<CancellationToken>()))
                .Callback<PaymentReportDocument, InsertOneOptions?, CancellationToken>(
                    (document, _, _) => insertedDocument = document)
                .Returns(Task.CompletedTask);

            // Act
            await _sut.InsertAsync(report, cancellationToken);

            // Assert
            insertedDocument.Should().NotBeNull();
            insertedDocument!.ToDomain().Should().Be(report);
            _collectionMock.Verify(collection => collection.InsertOneAsync(
                    It.IsAny<PaymentReportDocument>(),
                    It.IsAny<InsertOneOptions?>(),
                    It.Is<CancellationToken>(token => token == cancellationToken)),
                Times.Once);
        }

        [Fact]
        public async Task Given_Reports_When_GetSummaryAsyncIsCalled_Then_Should_Return_Totalized_Summary()
        {
            // Arrange
            var documents = new List<PaymentReportDocument>
            {
                CreateDocument(PaymentStatus.Approved, 100m, DateTime.UtcNow),
                CreateDocument(PaymentStatus.Approved, 25m, DateTime.UtcNow.AddMinutes(-1)),
                CreateDocument(PaymentStatus.Rejected, 40m, DateTime.UtcNow.AddMinutes(-2)),
                CreateDocument(PaymentStatus.Pending, 15m, DateTime.UtcNow.AddMinutes(-3))
            };

            SetupFindAsync(documents);

            // Act
            var summary = await _sut.GetSummaryAsync(CancellationToken.None);

            // Assert
            summary.Should().Be(new PaymentReportSummary(
                TotalPayments: 4,
                TotalApproved: 2,
                TotalRejected: 1,
                ApprovedAmount: 125m,
                RejectedAmount: 40m));
        }

        [Fact]
        public async Task Given_Reports_When_GetPagedAsyncIsCalled_Then_Should_Return_Domain_Page_And_TotalCount()
        {
            // Arrange
            var documents = new List<PaymentReportDocument>
            {
                CreateDocument(PaymentStatus.Approved, 60m, DateTime.UtcNow.AddMinutes(-2)),
                CreateDocument(PaymentStatus.Rejected, 30m, DateTime.UtcNow.AddMinutes(-3))
            };
            FindOptions<PaymentReportDocument, PaymentReportDocument>? capturedOptions = null;

            SetupCountDocuments(5);
            SetupFindAsync(documents, (_, options, _) => capturedOptions = options);

            // Act
            var result = await _sut.GetPagedAsync(pageNumber: 2, pageSize: 2, CancellationToken.None);

            // Assert
            result.TotalCount.Should().Be(5);
            result.Reports.Should().Equal(documents.Select(document => document.ToDomain()));
            capturedOptions.Should().NotBeNull();
            capturedOptions!.Skip.Should().Be(2);
            capturedOptions.Limit.Should().Be(2);
            capturedOptions.Sort.Should().NotBeNull();
        }

        [Fact]
        public async Task Given_UserId_When_GetByUserIdAsyncIsCalled_Then_Should_Return_Filtered_Domain_Page()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var documents = new List<PaymentReportDocument>
            {
                CreateDocument(PaymentStatus.Approved, 80m, DateTime.UtcNow, userId)
            };
            FilterDefinition<PaymentReportDocument>? countFilter = null;
            FilterDefinition<PaymentReportDocument>? findFilter = null;

            SetupCountDocuments(1, filter => countFilter = filter);
            SetupFindAsync(documents, (filter, _, _) => findFilter = filter);

            // Act
            var result = await _sut.GetByUserIdAsync(userId, pageNumber: 1, pageSize: 10, CancellationToken.None);

            // Assert
            result.TotalCount.Should().Be(1);
            result.Reports.Should().ContainSingle().Which.UserId.Should().Be(userId);
            countFilter.Should().NotBeNull();
            findFilter.Should().BeSameAs(countFilter);
        }

        private void SetupCountDocuments(
            long totalCount,
            Action<FilterDefinition<PaymentReportDocument>>? onCount = null)
        {
            _collectionMock
                .Setup(collection => collection.CountDocumentsAsync(
                    It.IsAny<FilterDefinition<PaymentReportDocument>>(),
                    It.IsAny<CountOptions?>(),
                    It.IsAny<CancellationToken>()))
                .Callback<FilterDefinition<PaymentReportDocument>, CountOptions?, CancellationToken>(
                    (filter, _, _) => onCount?.Invoke(filter))
                .ReturnsAsync(totalCount);
        }

        private void SetupFindAsync(
            IReadOnlyList<PaymentReportDocument> documents,
            Action<FilterDefinition<PaymentReportDocument>, FindOptions<PaymentReportDocument, PaymentReportDocument>, CancellationToken>? onFind = null)
        {
            _collectionMock
                .Setup(collection => collection.FindAsync(
                    It.IsAny<FilterDefinition<PaymentReportDocument>>(),
                    It.IsAny<FindOptions<PaymentReportDocument, PaymentReportDocument>>(),
                    It.IsAny<CancellationToken>()))
                .Callback<FilterDefinition<PaymentReportDocument>, FindOptions<PaymentReportDocument, PaymentReportDocument>, CancellationToken>(
                    (filter, options, cancellationToken) => onFind?.Invoke(filter, options, cancellationToken))
                .ReturnsAsync(CreateCursor(documents).Object);
        }

        private static Mock<IAsyncCursor<PaymentReportDocument>> CreateCursor(
            IReadOnlyList<PaymentReportDocument> documents)
        {
            var cursorMock = new Mock<IAsyncCursor<PaymentReportDocument>>();
            cursorMock.SetupSequence(cursor => cursor.MoveNext(It.IsAny<CancellationToken>()))
                .Returns(true)
                .Returns(false);
            cursorMock.SetupSequence(cursor => cursor.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);
            cursorMock.SetupGet(cursor => cursor.Current).Returns(documents);

            return cursorMock;
        }

        private static PaymentReport CreateReport(
            PaymentStatus status,
            decimal amount,
            DateTime processedAt,
            Guid? userId = null)
        {
            return new PaymentReport(
                Guid.NewGuid(),
                userId ?? Guid.NewGuid(),
                Guid.NewGuid(),
                amount,
                status,
                processedAt);
        }

        private static PaymentReportDocument CreateDocument(
            PaymentStatus status,
            decimal amount,
            DateTime processedAt,
            Guid? userId = null)
        {
            return PaymentReportDocument.FromDomain(CreateReport(status, amount, processedAt, userId));
        }
    }
}
