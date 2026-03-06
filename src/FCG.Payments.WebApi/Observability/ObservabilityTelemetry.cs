using System.Diagnostics;
using System.Diagnostics.Metrics;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace FCG.Payments.WebApi.Observability
{
    public static class ObservabilityTelemetry
    {
        public const string ActivitySourceName = "FCG.Payments";
        public const string MeterName = "FCG.Payments";
        public static readonly ActivitySource ActivitySource = new(ActivitySourceName);
        public static readonly Meter Meter = new(MeterName);

        public static ResourceBuilder CreateResourceBuilder(ObservabilityOptions options, string environmentName)
        {
            return ResourceBuilder.CreateDefault()
                .AddService(
                    serviceName: options.ServiceName,
                    serviceVersion: options.ServiceVersion,
                    serviceInstanceId: Environment.MachineName)
                .AddAttributes(new Dictionary<string, object>
                {
                    ["deployment.environment"] = environmentName,
                    ["service.namespace"] = "FCG"
                });
        }

        public static void ConfigureTracing(TracerProviderBuilder tracing, ResourceBuilder resourceBuilder, ObservabilityOptions options)
        {
            tracing
                .SetResourceBuilder(resourceBuilder)
                .AddSource(ActivitySourceName)
                .AddAspNetCoreInstrumentation(instrumentationOptions =>
                {
                    instrumentationOptions.RecordException = true;
                    instrumentationOptions.Filter = context => !context.Request.Path.StartsWithSegments("/health");
                })
                .AddHttpClientInstrumentation(instrumentationOptions =>
                {
                    instrumentationOptions.RecordException = true;
                })
                .AddSqlClientInstrumentation(instrumentationOptions =>
                {
                    instrumentationOptions.RecordException = true;
                    instrumentationOptions.SetDbStatementForText = true;
                });

            if (!options.EnableOtlpExporter)
            {
                return;
            }

            tracing.AddOtlpExporter(exporterOptions =>
            {
                exporterOptions.Endpoint = new Uri(options.OtlpEndpoint);
            });
        }

        public static void ConfigureMetrics(MeterProviderBuilder metrics, ResourceBuilder resourceBuilder, ObservabilityOptions options)
        {
            metrics
                .SetResourceBuilder(resourceBuilder)
                .AddMeter(MeterName)
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation();

            if (!options.EnableOtlpExporter)
            {
                return;
            }

            metrics.AddOtlpExporter(exporterOptions =>
            {
                exporterOptions.Endpoint = new Uri(options.OtlpEndpoint);
            });
        }
    }
}
