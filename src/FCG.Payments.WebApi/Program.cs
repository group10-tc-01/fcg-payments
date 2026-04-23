using FCG.Payments.Application.DependencyInjection;
using FCG.Payments.Infrastructure.Auth.DependencyInjection;
using FCG.Payments.Infrastructure.Kafka.DependencyInjection;
using FCG.Payments.Infrastructure.MongoDb.DependencyInjection;
using FCG.Payments.Infrastructure.SqlServer.DependencyInjection;
using FCG.Payments.WebApi.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace FCG.Payments.WebApi
{
    [ExcludeFromCodeCoverage]
    public class Program
    {
        protected Program() { }

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddWebApi(builder.Configuration);

            builder.Services.AddApplication();

            builder.Services.AddSqlServerInfrastructure(builder.Configuration);

            builder.Services.AddMongoDbInfrastructure(builder.Configuration);

            builder.Services.AddAuthInfrastruture(builder.Configuration);

            builder.Services.AddKafkaInfrastructure(builder.Configuration);

            var app = builder.Build();

            app.UseWebApiPipeline();

            app.Run();
        }
    }
}
