namespace FCG.Payments.Infrastructure.Kafka.Settings
{
    public sealed class KafkaSettings
    {
        public string BootstrapServers { get; set; } = string.Empty;
        public string GroupId { get; set; } = string.Empty;
        public int ConsumerTimeoutMs { get; set; } = 100;
        public KafkaTopics Topics { get; set; } = new();
    }

    public sealed class KafkaTopics
    {
        public string UserCreated { get; set; } = string.Empty;
        public string OrderPlaced { get; set; } = string.Empty;
    }
}
