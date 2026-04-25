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
            return GetPagedAsync(
                new PaymentReportFilter(null, null, null, userId, null),
                pageNumber,
                pageSize,
                cancellationToken);
        }

        public Task<(IEnumerable<PaymentReport> Reports, int TotalCount)> GetPagedAsync(
            PaymentReportFilter filter,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            return GetPagedAsync(
                BuildFilter(filter),
                pageNumber,
                pageSize,
                cancellationToken);
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

        public async Task<IReadOnlyList<PaymentReport>> GetAsync(
            PaymentReportFilter filter,
            int limit,
            CancellationToken cancellationToken = default)
        {
            var documents = await _collection
                .Find(BuildFilter(filter))
                .SortByDescending(report => report.ProcessedAt)
                .Limit(limit)
                .ToListAsync(cancellationToken);

            return documents.Select(report => report.ToDomain()).ToList().AsReadOnly();
        }

        public Task<PaymentReportSummary> GetSummaryAsync(CancellationToken cancellationToken = default)
        {
            return GetSummaryAsync(PaymentReportFilter.Empty, cancellationToken);
        }

        public async Task<PaymentReportSummary> GetSummaryAsync(
            PaymentReportFilter filter,
            CancellationToken cancellationToken = default)
        {
            var mongoFilter = BuildFilter(filter);
            var approvedFilter = Builders<PaymentReportDocument>.Filter.And(
                mongoFilter,
                Builders<PaymentReportDocument>.Filter.Eq(report => report.Status, PaymentStatus.Approved));
            var rejectedFilter = Builders<PaymentReportDocument>.Filter.And(
                mongoFilter,
                Builders<PaymentReportDocument>.Filter.Eq(report => report.Status, PaymentStatus.Rejected));

            var totalCount = (int)await _collection.CountDocumentsAsync(mongoFilter, cancellationToken: cancellationToken);

            var approvedReports = await _collection
                .Find(approvedFilter)
                .ToListAsync(cancellationToken);
            var rejectedReports = await _collection
                .Find(rejectedFilter)
                .ToListAsync(cancellationToken);

            return new PaymentReportSummary(
                totalCount,
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

        private static FilterDefinition<PaymentReportDocument> BuildFilter(PaymentReportFilter filter)
        {
            var builder = Builders<PaymentReportDocument>.Filter;
            var filters = new List<FilterDefinition<PaymentReportDocument>>();

            if (filter.Status is not null)
            {
                filters.Add(builder.Eq(report => report.Status, filter.Status.Value));
            }

            if (filter.DateFrom is not null)
            {
                filters.Add(builder.Gte(report => report.ProcessedAt, filter.DateFrom.Value));
            }

            if (filter.DateTo is not null)
            {
                filters.Add(builder.Lte(report => report.ProcessedAt, NormalizeDateTo(filter.DateTo.Value)));
            }

            if (filter.UserId is not null)
            {
                filters.Add(builder.Eq(report => report.UserId, filter.UserId.Value));
            }

            if (filter.GameId is not null)
            {
                filters.Add(builder.Eq(report => report.GameId, filter.GameId.Value));
            }

            return filters.Count == 0 ? builder.Empty : builder.And(filters);
        }

        private static DateTime NormalizeDateTo(DateTime dateTo)
        {
            return dateTo.TimeOfDay == TimeSpan.Zero
                ? dateTo.Date.AddDays(1).AddTicks(-1)
                : dateTo;
        }
    }
}
