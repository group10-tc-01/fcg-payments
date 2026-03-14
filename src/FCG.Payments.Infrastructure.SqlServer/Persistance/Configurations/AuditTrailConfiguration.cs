using FCG.Payments.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FCG.Payments.Infrastructure.SqlServer.Configurations
{
    public class AuditTrailConfiguration : IEntityTypeConfiguration<AuditTrail>
    {
        public void Configure(EntityTypeBuilder<AuditTrail> builder)
        {
            builder.ToTable("audit_trails");
            builder.HasKey(e => e.Id);
            builder.HasIndex(e => e.EntityName);

            builder.Property(e => e.Id).ValueGeneratedNever();
            builder.Property(e => e.UserId);
            builder.Property(e => e.EntityName).HasMaxLength(100).IsRequired();
            builder.Property(e => e.PrimaryKey).HasMaxLength(100);
            builder.Property(e => e.DateUtc).IsRequired();
            builder.Property(e => e.TrailType).HasConversion<string>();

            // Apenas propriedades string, sem conversão
            builder.Property(e => e.OldValues).HasColumnType("nvarchar(max)");
            builder.Property(e => e.NewValues).HasColumnType("nvarchar(max)");
            builder.Property(e => e.ChangedColumns).HasColumnType("nvarchar(max)");
        }
    }
}
