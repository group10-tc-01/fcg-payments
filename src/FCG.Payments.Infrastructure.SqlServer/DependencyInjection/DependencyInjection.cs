using FCG.Payments.Domain.Abstractions;
using FCG.Payments.Infrastructure.SqlServer.Persistance;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace FCG.Payments.Infrastructure.SqlServer.DependencyInjection
{
    [ExcludeFromCodeCoverage]
    public static class DependencyInjection
    {
        public static IServiceCollection AddSqlServerInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSqlServer(configuration);
            services.AddRepositories();

            return services;
        }

        private static void AddSqlServer(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<FcgUserDbContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            });
        }

        private static void AddRepositories(this IServiceCollection services)
        {

            services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<FcgUserDbContext>());
        }
    }
}
