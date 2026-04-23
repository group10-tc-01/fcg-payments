using FCG.Payments.Infrastructure.MongoDb.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using System.Diagnostics.CodeAnalysis;

namespace FCG.Payments.Infrastructure.MongoDb.DependencyInjection
{
    [ExcludeFromCodeCoverage]
    public static class DependencyInjection
    {
        public static IServiceCollection AddMongoDbInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var mongoDbSection = configuration.GetSection(MongoDbSettings.SectionName);
            var mongoDbSettings = mongoDbSection.Get<MongoDbSettings>() ?? new MongoDbSettings();

            services.Configure<MongoDbSettings>(settings => mongoDbSection.Bind(settings));

            services.AddSingleton(_ => new MongoClient(mongoDbSettings.ConnectionString));
            services.AddSingleton<IMongoClient>(sp => sp.GetRequiredService<MongoClient>());
            services.AddScoped(sp =>
                sp.GetRequiredService<IMongoClient>().GetDatabase(mongoDbSettings.DatabaseName));

            services.AddHealthChecks()
                .AddMongoDb(
                    clientFactory: sp => sp.GetRequiredService<MongoClient>(),
                    databaseNameFactory: _ => mongoDbSettings.DatabaseName,
                    name: "mongodb");

            return services;
        }
    }
}
