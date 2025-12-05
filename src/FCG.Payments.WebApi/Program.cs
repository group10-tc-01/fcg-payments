using FCG.Payments.Application.DependencyInjection;
using FCG.Payments.Infrastructure.Auth.DependencyInjection;
using FCG.Payments.Infrastructure.SqlServer.DependencyInjection;
using FCG.Payments.WebApi.DependencyInjection;
using FCG.Payments.WebApi.Extensions;
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

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddHttpContextAccessor();

            builder.Services.AddWebApi(builder.Configuration);
            builder.Services.AddApplication();
            builder.Services.AddSqlServerInfrastructure(builder.Configuration);
            builder.Services.AddAuthInfrastruture(builder.Configuration);

            var app = builder.Build();

            var logger = app.Services.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Application started successfully");
            logger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);

            if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Docker")
            {
                //app.ApplyMigrations();
                //logger.LogInformation("Migrations applied");
            }

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseCustomerExceptionHandler();
            app.UseGlobalCorrelationId();

            //app.MapHealthChecks("/health", new HealthCheckOptions
            //{
            //    AllowCachingResponses = false,
            //    ResultStatusCodes =
            //    {
            //        [HealthStatus.Healthy] = StatusCodes.Status200OK,
            //        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable,
            //    }

            //});

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();

        }
    }
}