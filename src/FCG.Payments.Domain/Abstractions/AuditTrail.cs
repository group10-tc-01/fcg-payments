namespace FCG.Payments.Domain.Abstractions
{
    public sealed class AuditTrail
    {
        public Guid Id { get; private set; }
        public Guid? PerformedByUserId { get; private set; }
        public string EntityName { get; private set; } = string.Empty;
        public string? PrimaryKey { get; private set; }
        public AuditTrailType TrailType { get; private set; }
        public DateTimeOffset DateUtc { get; private set; }

        public string OldValues { get; private set; } = string.Empty;
        public string NewValues { get; private set; } = string.Empty;
        public string ChangedColumns { get; private set; } = string.Empty;

        private AuditTrail() { }

        public AuditTrail(
            Guid? performedByUserId,
            string entityName,
            string? primaryKey,
            AuditTrailType trailType,
            string oldValues = "",
            string newValues = "",
            string changedColumns = "")
        {
            Id = Guid.NewGuid();
            PerformedByUserId = performedByUserId;
            EntityName = entityName;
            PrimaryKey = primaryKey;
            TrailType = trailType;
            DateUtc = DateTimeOffset.UtcNow;
            OldValues = oldValues;
            NewValues = newValues;
            ChangedColumns = changedColumns;
        }
    }

    public enum AuditTrailType
    {
        Create,
        Update,
        Delete
    }
}
