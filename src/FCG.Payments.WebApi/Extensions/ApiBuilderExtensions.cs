using FCG.Payments.Infrastructure.SqlServer.Persistance;
using FCG.Payments.WebApi.Middlewares;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace FCG.Payments.WebApi.Extensions
{
    [ExcludeFromCodeCoverage]
    public static class ApiBuilderExtensions
    {
        public static void ApplyMigrations(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();

            using var dbContext = scope.ServiceProvider.GetRequiredService<FcgPaymentDbContext>();

            dbContext.Database.Migrate();
        }

        public static void UseCustomerExceptionHandler(this IApplicationBuilder app)
        {
            app.UseMiddleware<GlobalExceptionMiddleware>();
        }

        public static void UseGlobalCorrelationId(this IApplicationBuilder app)
        {
            app.UseMiddleware<GlobalCorrelationIdMiddleware>();
        }
    }
}
