using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace FCG.Payments.Application.Observability
{
    public static class ApplicationMetrics
    {
        private static readonly Meter Meter = new("FCG.Payments");

        private static readonly Counter<long> CommandExecutionsCounter = Meter.CreateCounter<long>(
            name: "application_command_executions_total",
            unit: "commands");

        private static readonly Counter<long> CommandFailuresCounter = Meter.CreateCounter<long>(
            name: "application_command_failures_total",
            unit: "commands");

        private static readonly Histogram<double> CommandDuration = Meter.CreateHistogram<double>(
            name: "application_command_duration_ms",
            unit: "ms");

        public static void RecordExecution(string commandName, bool success, double durationMs)
        {
            TagList tags = new TagList
            {
                { "command", commandName },
                { "success", success }
            };

            CommandExecutionsCounter.Add(1, tags);
            CommandDuration.Record(durationMs, tags);
        }

        public static void RecordFailure(string commandName, string exceptionType, double durationMs)
        {
            TagList tags = new TagList
            {
                { "command", commandName },
                { "exception_type", exceptionType }
            };

            CommandFailuresCounter.Add(1, tags);
            CommandDuration.Record(durationMs, tags);
        }
    }
}
