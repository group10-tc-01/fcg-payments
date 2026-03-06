using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace FCG.Payments.WebApi.Observability
{
    public static class ObservabilityTelemetry
    {
        public const string ActivitySourceName = "FCG.Payments";
        public const string MeterName = "FCG.Payments";

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
                .AddSource(ActivitySourceName);

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
                .AddMeter(MeterName);

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
