using FCG.Payments.Application.Abstractions.Audit;
using FCG.Payments.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace FCG.Payments.Infrastructure.SqlServer.Persistance.Interceptors
{
    public sealed class AuditingInterceptor(ICurrentSessionProvider currentSessionProvider, ILogger<AuditingInterceptor> logger) : SaveChangesInterceptor
    {
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            if (eventData.Context is FcgPaymentDbContext context)
            {
                context.ChangeTracker.DetectChanges();

                foreach (var entry in GetAuditEntries(context))
                    context.AuditTrail.Add(entry);
            }

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private List<AuditTrail> GetAuditEntries(FcgPaymentDbContext context)
        {
            var auditEntries = new List<AuditTrail>();
            var userId = currentSessionProvider.GetUserId();

            foreach (var entry in context.ChangeTracker.Entries<BaseEntity>())
            {
                if (entry.Entity is not IAuditableEntity)
                    continue;

                if (entry.State is EntityState.Detached or EntityState.Unchanged)
                    continue;

                var trailType = entry.State switch
                {
                    EntityState.Added => AuditTrailType.Create,
                    EntityState.Modified => AuditTrailType.Update,
                    EntityState.Deleted => AuditTrailType.Delete,
                    _ => (AuditTrailType?)null
                };

                if (trailType is null)
                {
                    logger.LogWarning(
                        "[AuditingInterceptor] Unsupported entity state {EntityState} for {EntityName} (ID: {EntityId}).",
                        entry.State, entry.Entity.GetType().Name, entry.Entity.Id);
                    continue;
                }

                var owned = GetOwnedEntries(entry);

                auditEntries.Add(new AuditTrail(
                    userId,
                    entry.Entity.GetType().Name,
                    entry.Entity.Id.ToString(),
                    trailType.Value,
                    GetValuesAsJson(entry, owned, useOriginalValues: true,  includeAll: entry.State == EntityState.Deleted,  onlyForStates: [EntityState.Modified, EntityState.Deleted]),
                    GetValuesAsJson(entry, owned, useOriginalValues: false, includeAll: entry.State == EntityState.Added,    onlyForStates: [EntityState.Added, EntityState.Modified]),
                    GetChangedColumnsAsJson(entry, owned)));
            }

            return auditEntries;
        }

        private static string GetValuesAsJson(
            EntityEntry<BaseEntity> entry,
            IReadOnlyCollection<(string Nav, EntityEntry E)> ownedEntries,
            bool useOriginalValues,
            bool includeAll,
            EntityState[] onlyForStates)
        {
            if (!onlyForStates.Contains(entry.State))
                return string.Empty;

            var values = new Dictionary<string, object?>();

            foreach (var prop in entry.Properties)
            {
                if (prop.IsTemporary || prop.Metadata.IsForeignKey())
                    continue;

                if (!includeAll && Equals(prop.OriginalValue, prop.CurrentValue))
                    continue;

                values[PropertyName(null, prop)] = useOriginalValues ? prop.OriginalValue : prop.CurrentValue;
            }

            foreach (var (nav, ownedEntry) in ownedEntries)
            {
                foreach (var prop in ownedEntry.Properties)
                {
                    if (prop.IsTemporary || prop.Metadata.IsForeignKey() || prop.Metadata.IsPrimaryKey())
                        continue;

                    if (!includeAll && Equals(prop.OriginalValue, prop.CurrentValue))
                        continue;

                    values[PropertyName(nav, prop)] = useOriginalValues ? prop.OriginalValue : prop.CurrentValue;
                }
            }

            return values.Count > 0 ? JsonSerializer.Serialize(values) : string.Empty;
        }

        private static string GetChangedColumnsAsJson(
            EntityEntry<BaseEntity> entry,
            IReadOnlyCollection<(string Nav, EntityEntry E)> ownedEntries)
        {
            if (entry.State != EntityState.Modified)
                return string.Empty;

            var changed = entry.Properties
                .Where(p => !p.IsTemporary && !p.Metadata.IsForeignKey() && !Equals(p.OriginalValue, p.CurrentValue))
                .Select(p => PropertyName(null, p))
                .ToList();

            foreach (var (nav, ownedEntry) in ownedEntries)
            {
                changed.AddRange(ownedEntry.Properties
                    .Where(p => !p.IsTemporary && !p.Metadata.IsForeignKey() && !p.Metadata.IsPrimaryKey()
                                && !Equals(p.OriginalValue, p.CurrentValue))
                    .Select(p => PropertyName(nav, p)));
            }

            return changed.Count > 0 ? JsonSerializer.Serialize(changed) : string.Empty;
        }

        private static IReadOnlyCollection<(string Nav, EntityEntry E)> GetOwnedEntries(EntityEntry<BaseEntity> ownerEntry)
        {
            return ownerEntry.References
                .Where(r => r.TargetEntry?.Metadata.IsOwned() == true)
                .Select(r => (r.Metadata.Name, r.TargetEntry!))
                .ToList();
        }

        private static string PropertyName(string? prefix, PropertyEntry property)
        {
            if (string.IsNullOrWhiteSpace(prefix))
                return property.Metadata.Name;

            return property.Metadata.Name == "Value" ? prefix : $"{prefix}.{property.Metadata.Name}";
        }
    }
}
