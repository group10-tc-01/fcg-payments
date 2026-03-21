using FCG.Payments.Application.Abstractions.Audit;
using FCG.Payments.Domain.Abstractions;
using FCG.Payments.Domain.Payments;
using FCG.Payments.Domain.Wallets;
using FCG.Payments.Infrastructure.SqlServer.Persistance;
using FCG.Payments.Infrastructure.SqlServer.Persistance.Interceptors;
using FCG.Payments.Infrastructure.SqlServer.Persistance.Repositories;
using FCG.Payments.Infrastructure.SqlServer.Providers;
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
            services.AddProviders();
            services.AddInterceptors();
            services.AddSqlServer(configuration);
            services.AddRepositories();

            return services;
        }

        private static void AddProviders(this IServiceCollection services)
        {
            services.AddScoped<ICurrentSessionProvider, CurrentSessionProvider>();
        }

        private static void AddInterceptors(this IServiceCollection services)
        {
            services.AddScoped<AuditingInterceptor>();
        }

        private static void AddSqlServer(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<FcgPaymentDbContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            });
        }

        private static void AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IWriteOnlyWalletRepository, WalletRepository>();
            services.AddScoped<IReadOnlyWalletRepository, WalletRepository>();

            services.AddScoped<IWriteOnlyPaymentRepository, PaymentRepository>();
            services.AddScoped<IReadOnlyPaymentRepository, PaymentRepository>();

            services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<FcgPaymentDbContext>());
        }
    }
}
