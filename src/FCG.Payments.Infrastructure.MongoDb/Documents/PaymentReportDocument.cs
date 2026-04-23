using FCG.Payments.Domain.Payments;
using FCG.Payments.Domain.Payments.Reports;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FCG.Payments.Infrastructure.MongoDb.Documents
{
    public sealed class PaymentReportDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid PaymentId { get; set; }

        [BsonRepresentation(BsonType.String)]
        public Guid UserId { get; set; }

        [BsonRepresentation(BsonType.String)]
        public Guid GameId { get; set; }

        [BsonRepresentation(BsonType.Decimal128)]
        public decimal Amount { get; set; }

        [BsonRepresentation(BsonType.String)]
        public PaymentStatus Status { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime ProcessedAt { get; set; }

        public static PaymentReportDocument FromDomain(PaymentReport report)
        {
            return new PaymentReportDocument
            {
                PaymentId = report.PaymentId,
                UserId = report.UserId,
                GameId = report.GameId,
                Amount = report.Amount,
                Status = report.Status,
                ProcessedAt = report.ProcessedAt
            };
        }

        public PaymentReport ToDomain()
        {
            return new PaymentReport(
                PaymentId,
                UserId,
                GameId,
                Amount,
                Status,
                ProcessedAt);
        }
    }
}
