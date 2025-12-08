using FCG.Payments.CommomTestUtilities.Builders.Payments;
using FCG.Payments.CommomTestUtilities.Builders.Wallets;
using FCG.Payments.Domain.Payments;
using FCG.Payments.Domain.Wallets;
using FCG.Payments.Infrastructure.SqlServer.Persistance;
using FCG.Payments.WebApi;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace FCG.Payments.IntegratedTests.Configurations
{
    [ExcludeFromCodeCoverage]
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        private DbConnection? _connection;
        public List<Wallet> CreatedWallets { get; private set; } = [];
        public List<Payment> CreatedPayments { get; private set; } = [];

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Test").ConfigureServices(services =>
            {
                RemoveEntityFrameworkServices(services);
                RemoveKafkaServices(services);

                _connection?.Dispose();
                _connection = new SqliteConnection("Data Source=:memory:");
                _connection.Open();

                services.AddDbContext<FcgPaymentDbContext>(options =>
                {
                    options.UseSqlite(_connection)
                            .EnableSensitiveDataLogging()
                            .EnableDetailedErrors();
                });

                EnsureDatabaseSeeded(services);
            });
        }

        private static void RemoveEntityFrameworkServices(IServiceCollection services)
        {
            var descriptorsToRemove = services.Where(d =>
                d.ServiceType == typeof(DbContextOptions<FcgPaymentDbContext>) ||
                d.ServiceType == typeof(FcgPaymentDbContext) ||
                d.ServiceType.Namespace?.StartsWith("Microsoft.EntityFrameworkCore") == true)
                .ToList();

            foreach (var descriptor in descriptorsToRemove)
            {
                services.Remove(descriptor);
            }

        }

        private static void RemoveKafkaServices(IServiceCollection services)
        {
            var kafkaDescriptorsToRemove = services.Where(d =>
                d.ServiceType.FullName?.Contains("Kafka") == true ||
                d.ImplementationType?.FullName?.Contains("Kafka") == true)
                .ToList();

            foreach (var descriptor in kafkaDescriptorsToRemove)
            {
                services.Remove(descriptor);
            }
        }

        private void EnsureDatabaseSeeded(IServiceCollection services)
        {
            using var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<FcgPaymentDbContext>();

            Log.Information("Seeding database for integrated tests");

            dbContext.Database.EnsureDeleted();
            dbContext.Database.EnsureCreated();

            StartDatabase(dbContext);
        }

        private void StartDatabase(FcgPaymentDbContext context)
        {
            var itemsQuantity = 2;

            Log.Information($"Creating {itemsQuantity} items for integrated test");

            CreatedWallets = CreateWallets(context, itemsQuantity);
            CreatedPayments = CreatePayments(context, CreatedWallets);
        }

        private List<Wallet> CreateWallets(FcgPaymentDbContext context, int itemsQuantity)
        {
            var wallets = new List<Wallet>();

            for (int i = 1; i <= itemsQuantity; i++)
            {
                var wallet = new WalletBuilder().Build();
                wallets.Add(wallet);
            }

            context.Wallet.AddRange(wallets);
            context.SaveChanges();
            Log.Information("Created {Count} wallets", wallets.Count);

            return wallets;
        }

        private List<Payment> CreatePayments(FcgPaymentDbContext context, List<Wallet> wallets)
        {
            var payments = new List<Payment>();

            foreach (var wallet in wallets)
            {
                var payment = new PaymentBuilder().BuildWithParameters(
                    wallet.UserId,
                    Guid.NewGuid(),
                    wallet.Id,
                    100m);
                payments.Add(payment);
            }

            context.Payment.AddRange(payments);
            context.SaveChanges();
            Log.Information("Created {Count} payments", payments.Count);

            return payments;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _connection?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
