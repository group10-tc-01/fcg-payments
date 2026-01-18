using FCG.Payments.Domain.Payments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Diagnostics.CodeAnalysis;

namespace FCG.Payments.Infrastructure.SqlServer.Persistance.Configurations
{
    [ExcludeFromCodeCoverage]
    public class PaymentConfiguration : BaseConfiguration<Payment>
    {
        public override void Configure(EntityTypeBuilder<Payment> builder)
        {
            base.Configure(builder);

            builder.ToTable("Payment");

            builder.Property(p => p.CorrelationId)
                .IsRequired();

            builder.Property(p => p.UserId)
                .IsRequired();

            builder.Property(p => p.GameId)
                .IsRequired();

            builder.Property(p => p.WalletId)
                .IsRequired();

            builder.OwnsOne(p => p.Amount, amountBuilder =>
            {
                amountBuilder.Property(a => a.Value)
                    .HasColumnName("Amount")
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();
            });

            builder.Property(p => p.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(p => p.FailureReason)
                .HasMaxLength(500);

            builder.Property(p => p.ProcessedAt)
                .HasColumnType("datetime2");

            builder.HasIndex(p => p.UserId)
                .HasDatabaseName("IX_Payments_UserId");

            builder.HasIndex(p => p.Status)
                .HasDatabaseName("IX_Payments_Status");

            builder.HasIndex(p => p.CreatedAt)
                .IsDescending()
                .HasDatabaseName("IX_Payments_CreatedAt");

            builder.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Payment_Status", "[Status] IN ('Pending', 'Approved', 'Rejected')");
                t.HasCheckConstraint("CK_Payment_Amount", "[Amount] > 0");
            });

            builder.HasOne<Domain.Wallets.Wallet>()
                .WithMany()
                .HasForeignKey(p => p.WalletId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
