using System.Diagnostics;
using System.Diagnostics.Metrics;
using OpenTelemetry.Exporter;
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
                ConfigureOtlpExporter(exporterOptions, options, OtlpSignal.Traces);
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
                ConfigureOtlpExporter(exporterOptions, options, OtlpSignal.Metrics);
            });
        }

        private enum OtlpSignal { Traces, Metrics }

        private static void ConfigureOtlpExporter(OtlpExporterOptions exporterOptions, ObservabilityOptions options, OtlpSignal signal)
        {
            if (!string.IsNullOrWhiteSpace(options.OtlpAuthHeader))
            {
                // Grafana Cloud OTLP gateway requires the full signal path in the URL.
                // The SDK does NOT auto-append /v1/traces or /v1/metrics when Endpoint is
                // set explicitly, so we build the complete path here.
                string signalPath = signal == OtlpSignal.Traces ? "/otlp/v1/traces" : "/otlp/v1/metrics";
                exporterOptions.Endpoint = new Uri(options.OtlpEndpoint + signalPath);
                exporterOptions.Protocol = OtlpExportProtocol.HttpProtobuf;
                exporterOptions.Headers = $"Authorization={options.OtlpAuthHeader}";
            }
            else
            {
                // Local otel-collector via gRPC — use the endpoint as-is.
                exporterOptions.Endpoint = new Uri(options.OtlpEndpoint);
            }
        }
    }
}
