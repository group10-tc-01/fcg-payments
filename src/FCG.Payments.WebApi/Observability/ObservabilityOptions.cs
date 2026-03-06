namespace FCG.Payments.WebApi.Observability
{
    public sealed class ObservabilityOptions
    {
        public const string SectionName = "Observability";

        public string ServiceName { get; set; } = "FCG.Payments";

        public string ServiceVersion { get; set; } = "0.0.0-local";

        public string OtlpEndpoint { get; set; } = "http://localhost:4317";

        public bool EnableOtlpExporter { get; set; } = true;
    }
}
