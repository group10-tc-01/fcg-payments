using FCG.Payments.Application.Abstractions.Messaging;
using FCG.Payments.Infrastructure.Kafka.Consumers.OrderPlaced;
using FCG.Payments.Infrastructure.Kafka.Consumers.UserCreated;
using FCG.Payments.Infrastructure.Kafka.Producers;
using FCG.Payments.Infrastructure.Kafka.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace FCG.Payments.Infrastructure.Kafka.DependencyInjection
{
    [ExcludeFromCodeCoverage]
    public static class DependencyInjection
    {
        public static IServiceCollection AddKafkaInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var assembly = Assembly.GetExecutingAssembly();

            services.AddMediatR(config =>
            {
                config.RegisterServicesFromAssembly(assembly);
            });

            var kafkaSection = configuration.GetSection("KafkaSettings");
            var kafkaSettings = kafkaSection.Get<KafkaSettings>() ?? new KafkaSettings();

            ValidateKafkaSettings(kafkaSettings);

            services.Configure<KafkaSettings>(kafkaSection);

            services.AddSingleton<IMessageProducer, KafkaMessageProducer>();

            services.AddHostedService<UserCreatedConsumer>();
            services.AddHostedService<OrderPlacedConsumer>();

            return services;
        }

        private static void ValidateKafkaSettings(KafkaSettings settings)
        {
            if (settings.UseSaslSsl)
            {
                if (string.IsNullOrWhiteSpace(settings.SaslUsername))
                {
                    throw new InvalidOperationException("KafkaSettings:SaslUsername must be configured when UseSaslSsl is true.");
                }

                if (string.IsNullOrWhiteSpace(settings.SaslPassword))
                {
                    throw new InvalidOperationException("KafkaSettings:SaslPassword must be configured when UseSaslSsl is true.");
                }
            }
        }
    }
}
