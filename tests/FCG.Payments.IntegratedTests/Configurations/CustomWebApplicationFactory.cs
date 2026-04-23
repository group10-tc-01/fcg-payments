using FCG.Payments.CommomTestUtilities.Builders.Payments;
using FCG.Payments.CommomTestUtilities.Builders.Wallets;
using FCG.Payments.Domain.Payments;
using FCG.Payments.Domain.Payments.Reports;
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
        public List<PaymentReport> CreatedPaymentReports { get; private set; } = [];

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Test").ConfigureServices(services =>
            {
                RemoveEntityFrameworkServices(services);
                RemoveKafkaServices(services);
                services.AddScoped<IPaymentReportRepository>(_ => new InMemoryPaymentReportRepository(CreatedPaymentReports));

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
            CreatedPaymentReports = CreatedPayments
                .Select(payment => new PaymentReport(
                    payment.Id,
                    payment.UserId,
                    payment.GameId,
                    payment.Amount,
                    payment.Status,
                    payment.ProcessedAt ?? payment.CreatedAt))
                .ToList();
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
                    "teste@gmail.com",
                    Guid.NewGuid(),
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

        private sealed class InMemoryPaymentReportRepository : IPaymentReportRepository
        {
            private readonly List<PaymentReport> _reports;

            public InMemoryPaymentReportRepository(List<PaymentReport> reports)
            {
                _reports = reports;
            }

            public Task InsertAsync(PaymentReport report, CancellationToken cancellationToken = default)
            {
                _reports.Add(report);

                return Task.CompletedTask;
            }

            public Task<(IEnumerable<PaymentReport> Reports, int TotalCount)> GetByUserIdAsync(
                Guid userId,
                int pageNumber,
                int pageSize,
                CancellationToken cancellationToken = default)
            {
                var reports = _reports.Where(report => report.UserId == userId);

                return GetPagedAsync(reports, pageNumber, pageSize);
            }

            public Task<(IEnumerable<PaymentReport> Reports, int TotalCount)> GetPagedAsync(
                int pageNumber,
                int pageSize,
                CancellationToken cancellationToken = default)
            {
                return GetPagedAsync(_reports, pageNumber, pageSize);
            }

            public Task<PaymentReportSummary> GetSummaryAsync(CancellationToken cancellationToken = default)
            {
                var approvedReports = _reports.Where(report => report.Status == PaymentStatus.Approved).ToList();
                var rejectedReports = _reports.Where(report => report.Status == PaymentStatus.Rejected).ToList();

                return Task.FromResult(new PaymentReportSummary(
                    _reports.Count,
                    approvedReports.Count,
                    rejectedReports.Count,
                    approvedReports.Sum(report => report.Amount),
                    rejectedReports.Sum(report => report.Amount)));
            }

            private static Task<(IEnumerable<PaymentReport> Reports, int TotalCount)> GetPagedAsync(
                IEnumerable<PaymentReport> source,
                int pageNumber,
                int pageSize)
            {
                var reports = source
                    .OrderByDescending(report => report.ProcessedAt)
                    .ToList();

                var pagedReports = reports
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                return Task.FromResult(((IEnumerable<PaymentReport>)pagedReports, reports.Count));
            }
        }
    }
}
