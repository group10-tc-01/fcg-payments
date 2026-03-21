using FCG.Payments.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Diagnostics.CodeAnalysis;

namespace FCG.Payments.Infrastructure.SqlServer.Persistance.Configurations
{
    [ExcludeFromCodeCoverage]
    public class AuditTrailConfiguration : IEntityTypeConfiguration<AuditTrail>
    {
        public void Configure(EntityTypeBuilder<AuditTrail> builder)
        {
            builder.ToTable("AuditTrail");

            builder.HasKey(e => e.Id);

            builder.HasIndex(e => e.EntityName);

            builder.Property(e => e.Id).ValueGeneratedNever();

            builder.Property(e => e.EntityName)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(e => e.PrimaryKey)
                .HasMaxLength(100);

            builder.Property(e => e.DateUtc)
                .IsRequired();

            builder.Property(e => e.TrailType)
                .HasConversion<string>();

            builder.Property(e => e.OldValues)
                .IsRequired(false);

            builder.Property(e => e.NewValues)
                .IsRequired(false);

            builder.Property(e => e.ChangedColumns)
                .IsRequired(false);
        }
    }
}
