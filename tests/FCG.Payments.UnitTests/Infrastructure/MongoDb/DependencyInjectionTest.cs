using FCG.Payments.Infrastructure.MongoDb.DependencyInjection;
using FCG.Payments.Infrastructure.MongoDb.Settings;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace FCG.Payments.UnitTests.Infrastructure.MongoDb
{
    public class DependencyInjectionTest
    {
        [Fact]
        public void AddMongoDbInfrastructure_ShouldRegisterSettingsClientDatabaseAndHealthCheck()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["MongoDbSettings:ConnectionString"] = "mongodb://localhost:27017",
                    ["MongoDbSettings:DatabaseName"] = "Payments"
                })
                .Build();

            var services = new ServiceCollection();

            // Act
            services.AddMongoDbInfrastructure(configuration);

            using var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();

            // Assert
            provider.GetRequiredService<IOptions<MongoDbSettings>>()
                .Value
                .Should()
                .BeEquivalentTo(new MongoDbSettings
                {
                    ConnectionString = "mongodb://localhost:27017",
                    DatabaseName = "Payments"
                });

            provider.GetRequiredService<IMongoClient>()
                .Should()
                .BeSameAs(provider.GetRequiredService<IMongoClient>());

            provider.GetRequiredService<MongoClient>()
                .Should()
                .BeSameAs(provider.GetRequiredService<IMongoClient>());

            scope.ServiceProvider.GetRequiredService<IMongoDatabase>()
                .DatabaseNamespace
                .DatabaseName
                .Should()
                .Be("Payments");

            provider.GetRequiredService<IOptions<HealthCheckServiceOptions>>()
                .Value
                .Registrations
                .Should()
                .ContainSingle(registration => registration.Name == "mongodb");
        }
    }
}
