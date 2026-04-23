using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace QuickBite.Payment.Configurations
{
    public class PaymentEntityConfiguration : IEntityTypeConfiguration<Entities.Payment>
    {
        public void Configure(EntityTypeBuilder<Entities.Payment> builder)
        {
            builder.ToTable("Payments");
            builder.HasKey(p => p.PaymentId);
            builder.Property(p => p.PaymentId).ValueGeneratedOnAdd();

            builder.Property(p => p.OrderId).IsRequired();
            builder.HasIndex(p => p.OrderId).IsUnique(); // One payment per order typically

            builder.Property(p => p.CustomerId).IsRequired();
            builder.HasIndex(p => p.CustomerId);

            builder.Property(p => p.Amount).IsRequired();
            
            builder.Property(p => p.Status).IsRequired().HasMaxLength(20);
            builder.Property(p => p.Mode).IsRequired().HasMaxLength(20);
            builder.Property(p => p.TransactionId).HasMaxLength(100);
            builder.Property(p => p.Currency).HasMaxLength(5).HasDefaultValue("INR");
        }
    }
}
