using FCG.Payments.Domain.Wallets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Diagnostics.CodeAnalysis;

namespace FCG.Payments.Infrastructure.SqlServer.Persistance.Configurations
{
    [ExcludeFromCodeCoverage]
    public class WalletConfiguration : BaseConfiguration<Wallet>
    {
        public override void Configure(EntityTypeBuilder<Wallet> builder)
        {
            base.Configure(builder);

            builder.ToTable("Wallet");

            builder.Property(w => w.UserId)
                .IsRequired();

            builder.HasIndex(w => w.UserId)
                .IsUnique();

            builder.OwnsOne(w => w.Balance, balanceBuilder =>
            {
                balanceBuilder.Property(b => b.Value)
                    .HasColumnName("Balance")
                    .HasColumnType("decimal(18,2)")
                    .IsRequired()
                    .HasDefaultValue(1000.00m);
            });

            builder.ToTable(t => t.HasCheckConstraint("CK_Wallet_Balance", "[Balance] >= 0"));
        }
    }
}
