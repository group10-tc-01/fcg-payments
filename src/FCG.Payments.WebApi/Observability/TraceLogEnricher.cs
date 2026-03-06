using Serilog.Core;
using Serilog.Events;
using System.Diagnostics;

namespace FCG.Payments.WebApi.Observability
{
    public sealed class TraceLogEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            Activity? activity = Activity.Current;

            if (activity is null)
            {
                return;
            }

            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("trace_id", activity.TraceId.ToString()));
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("span_id", activity.SpanId.ToString()));

            if (activity.ParentSpanId != default)
            {
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("parent_span_id", activity.ParentSpanId.ToString()));
            }
        }
    }
}
