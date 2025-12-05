using FCG.Payments.Domain.Abstractions;
using FCG.Payments.Domain.Payments;
using FCG.Payments.Domain.Wallets;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Diagnostics.CodeAnalysis;

namespace FCG.Payments.Infrastructure.SqlServer.Persistance
{
    [ExcludeFromCodeCoverage]
    public class FcgPaymentDbContext : DbContext, IUnitOfWork
    {
        private readonly IPublisher _publisher;

        public DbSet<Wallet> Wallet { get; set; }
        public DbSet<Payment> Payment { get; set; }

        public FcgPaymentDbContext(DbContextOptions<FcgPaymentDbContext> options, IPublisher publisher) : base(options)
        {
            _publisher = publisher;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(FcgPaymentDbContext).Assembly);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var result = await base.SaveChangesAsync(cancellationToken);

            await PublishDomainEventsAsync();

            return result;
        }

        private async Task PublishDomainEventsAsync()
        {
            var domainEvents = ChangeTracker
                               .Entries<BaseEntity>()
                               .Select(entry => entry.Entity)
                               .SelectMany(entity =>
                               {
                                   var domainEvents = entity.GetDomainEvents();

                                   entity.ClearDomainEvents();

                                   return domainEvents;
                               }).ToList();

            foreach (var domainEvent in domainEvents)
            {
                await _publisher.Publish(domainEvent);
            }
        }
    }
}
