using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace FCG.Payments.Application.Observability
{
    public static class PaymentsMetrics
    {
        private static readonly Meter Meter = new("FCG.Payments");

        private static readonly Counter<long> PaymentsProcessedCounter = Meter.CreateCounter<long>(
            name: "payments_processed_total",
            unit: "payments");

        private static readonly Histogram<double> PaymentAmount = Meter.CreateHistogram<double>(
            name: "payment_amount",
            unit: "currency");

        public static void RecordProcessed(string status, decimal amount, string reason = "none")
        {
            TagList tags = new TagList
            {
                { "status", status },
                { "reason", reason }
            };

            PaymentsProcessedCounter.Add(1, tags);
            PaymentAmount.Record(decimal.ToDouble(amount), tags);
        }
    }
}
