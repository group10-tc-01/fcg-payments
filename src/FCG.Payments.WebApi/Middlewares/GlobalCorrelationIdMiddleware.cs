using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace FCG.Payments.WebApi.Middlewares
{

    [ExcludeFromCodeCoverage]
    public class GlobalCorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;
        private const string CorrelationIdHeaderName = "X-Correlation-Id";
        private const string CorrelationIdKey = "CorrelationId";

        public GlobalCorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var correlationId = GetOrCreateCorrelationId(context);

            context.Items[CorrelationIdKey] = correlationId;
            context.Response.Headers.Append(CorrelationIdHeaderName, correlationId);

            Activity? activity = Activity.Current;

            activity?.SetTag("correlation_id", correlationId);
            activity?.AddBaggage("correlation_id", correlationId);

            using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
            {
                await _next(context);
            }
        }

        private static string GetOrCreateCorrelationId(HttpContext context)
        {
            if (context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out var correlationId)
                && !string.IsNullOrWhiteSpace(correlationId))
            {
                return correlationId.ToString();
            }

            return Guid.NewGuid().ToString();
        }
    }
}
