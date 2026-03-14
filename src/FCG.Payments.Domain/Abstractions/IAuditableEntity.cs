namespace FCG.Payments.Domain.Abstractions
{
    public interface IAuditableEntity
    {
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public string CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
