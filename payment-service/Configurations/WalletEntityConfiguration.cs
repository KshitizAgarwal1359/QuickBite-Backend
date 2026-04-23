using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuickBite.Payment.Entities;

namespace QuickBite.Payment.Configurations
{
    public class WalletEntityConfiguration : IEntityTypeConfiguration<Wallet>
    {
        public void Configure(EntityTypeBuilder<Wallet> builder)
        {
            builder.ToTable("Wallets", t => t.HasCheckConstraint("CK_Wallet_Balance_NonNegative", "[Balance] >= 0"));
            builder.HasKey(w => w.WalletId);
            builder.Property(w => w.WalletId).ValueGeneratedOnAdd();

            builder.Property(w => w.CustomerId).IsRequired();
            builder.HasIndex(w => w.CustomerId).IsUnique(); // One wallet per customer

            builder.Property(w => w.Balance).IsRequired().HasDefaultValue(0);

            builder.HasMany(w => w.Statements)
                .WithOne(s => s.Wallet)
                .HasForeignKey(s => s.WalletId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
