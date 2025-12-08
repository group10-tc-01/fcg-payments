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

            services.Configure<KafkaSettings>(configuration.GetSection("KafkaSettings"));

            services.AddSingleton<IMessageProducer, KafkaMessageProducer>();

            services.AddHostedService<UserCreatedConsumer>();
            services.AddHostedService<OrderPlacedConsumer>();

            return services;
        }
    }
}
