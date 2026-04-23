using FCG.Payments.Domain.Payments;
using FCG.Payments.Domain.Payments.Reports;
using FCG.Payments.Infrastructure.MongoDb.Documents;
using MongoDB.Driver;

namespace FCG.Payments.Infrastructure.MongoDb.Repositories
{
    public sealed class PaymentReportMongoRepository : IPaymentReportRepository
    {
        private const string CollectionName = "payment_reports";
        private readonly IMongoCollection<PaymentReportDocument> _collection;

        public PaymentReportMongoRepository(IMongoDatabase database)
        {
            _collection = database.GetCollection<PaymentReportDocument>(CollectionName);
        }

        public async Task InsertAsync(PaymentReport report, CancellationToken cancellationToken = default)
        {
            await _collection.InsertOneAsync(
                PaymentReportDocument.FromDomain(report),
                cancellationToken: cancellationToken);
        }

        public Task<(IEnumerable<PaymentReport> Reports, int TotalCount)> GetByUserIdAsync(
            Guid userId,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var filter = Builders<PaymentReportDocument>.Filter.Eq(report => report.UserId, userId);

            return GetPagedAsync(filter, pageNumber, pageSize, cancellationToken);
        }

        public Task<(IEnumerable<PaymentReport> Reports, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            return GetPagedAsync(
                Builders<PaymentReportDocument>.Filter.Empty,
                pageNumber,
                pageSize,
                cancellationToken);
        }

        public async Task<PaymentReportSummary> GetSummaryAsync(CancellationToken cancellationToken = default)
        {
            var documents = await _collection
                .Find(Builders<PaymentReportDocument>.Filter.Empty)
                .ToListAsync(cancellationToken);

            var approvedReports = documents.Where(report => report.Status == PaymentStatus.Approved).ToList();
            var rejectedReports = documents.Where(report => report.Status == PaymentStatus.Rejected).ToList();

            return new PaymentReportSummary(
                documents.Count,
                approvedReports.Count,
                rejectedReports.Count,
                approvedReports.Sum(report => report.Amount),
                rejectedReports.Sum(report => report.Amount));
        }

        private async Task<(IEnumerable<PaymentReport> Reports, int TotalCount)> GetPagedAsync(
            FilterDefinition<PaymentReportDocument> filter,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken)
        {
            var totalCount = (int)await _collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);

            var documents = await _collection
                .Find(filter)
                .SortByDescending(report => report.ProcessedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync(cancellationToken);

            return (documents.Select(report => report.ToDomain()).ToList(), totalCount);
        }
    }
}
