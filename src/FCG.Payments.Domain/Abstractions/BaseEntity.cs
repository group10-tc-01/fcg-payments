namespace FCG.Payments.Domain.Abstractions
{
    public abstract class BaseEntity
    {
        public Guid Id { get; init; }
        public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; protected set; }

        protected BaseEntity() { }

        protected BaseEntity(Guid id)
        {
            Id = id;
        }

        public IReadOnlyList<IDomainEvent> GetDomainEvents() => _domainEvents.ToList();

        public void ClearDomainEvents() => _domainEvents.Clear();

        protected void RaiseDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

        private readonly List<IDomainEvent> _domainEvents = [];
    }
}
