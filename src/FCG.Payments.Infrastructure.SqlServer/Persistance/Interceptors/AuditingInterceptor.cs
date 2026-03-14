using FCG.Payments.Application.Audits;
using FCG.Payments.Domain.Abstractions;
using FCG.Payments.Infrastructure.SqlServer.Persistance.Providers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace FCG.Payments.Infrastructure.SqlServer.Persistance.Interceptors
{
    public class AuditingInterceptor(ICurrentSessionProvider currentSessionProvider, ILogger<AuditingInterceptor> logger) : SaveChangesInterceptor
    {
        private readonly ICurrentSessionProvider _currentSessionProvider = currentSessionProvider;
        private readonly ILogger<AuditingInterceptor> _logger = logger;

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            if (eventData.Context is FcgPaymentDbContext context)
            {
                var auditEntries = GetAuditEntries(context);

                foreach (var entry in auditEntries)
                {
                    context.AuditTrail.Add(entry);
                }
            }

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private List<AuditTrail> GetAuditEntries(FcgPaymentDbContext context)
        {
            var auditEntries = new List<AuditTrail>();
            var userId = _currentSessionProvider.GetUserId();

            foreach (var entry in context.ChangeTracker.Entries<BaseEntity>())
            {
                if (entry.Entity is not IAuditableEntity)
                    continue;

                if (entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                    continue;

                var entityName = entry.Entity.GetType().Name;
                var primaryKey = entry.Entity.Id.ToString();
                var trailType = entry.State switch
                {
                    EntityState.Added => AuditTrailType.Create,
                    EntityState.Modified => AuditTrailType.Update,
                    EntityState.Deleted => AuditTrailType.Delete,
                    _ => (AuditTrailType?)null
                };

                if (trailType is null)
                {
                    _logger.LogWarning(
                        "[AuditingInterceptor] Failed to determine audit trail type for entity {EntityName} (ID: {EntityId}). " +
                        "Entity state {EntityState} is not supported.",
                        entityName,
                        primaryKey,
                        entry.State);
                    continue;
                }

                var oldValues = GetChangedValuesAsJson(entry, original: true);
                var newValues = GetChangedValuesAsJson(entry, original: false);
                var changedColumns = GetChangedColumnsAsJson(entry);

                var auditTrail = new AuditTrail(
                    userId,
                    entityName,
                    primaryKey,
                    trailType.Value,
                    oldValues,
                    newValues,
                    changedColumns);

                auditEntries.Add(auditTrail);
            }

            return auditEntries;
        }

        private static string GetChangedValuesAsJson(EntityEntry<BaseEntity> entry, bool original)
        {
            var values = new Dictionary<string, object?>();

            foreach (var property in entry.Properties)
            {
                if (property.IsTemporary)
                    continue;

                var value = original ? property.OriginalValue : property.CurrentValue;
                values[property.Metadata.Name] = value;
            }

            return values.Count > 0 ? JsonSerializer.Serialize(values) : string.Empty;
        }

        private static string GetChangedColumnsAsJson(EntityEntry<BaseEntity> entry)
        {
            var changedColumns = entry.Properties
                .Where(p => p.IsModified && !p.IsTemporary)
                .Select(p => p.Metadata.Name)
                .ToList();

            return changedColumns.Count > 0 ? JsonSerializer.Serialize(changedColumns) : string.Empty;
        }
    }
}
