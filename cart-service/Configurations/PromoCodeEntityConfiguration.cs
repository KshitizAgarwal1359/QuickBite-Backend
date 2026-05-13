using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuickBite.Cart.Entities;

namespace QuickBite.Cart.Configurations
{
    public class PromoCodeEntityConfiguration : IEntityTypeConfiguration<PromoCode>
    {
        public void Configure(EntityTypeBuilder<PromoCode> builder)
        {
            builder.ToTable("PromoCodes");

            builder.HasKey(p => p.PromoCodeId);
            builder.Property(p => p.PromoCodeId).ValueGeneratedOnAdd();

            builder.Property(p => p.Code).IsRequired().HasMaxLength(50);
            builder.HasIndex(p => p.Code).IsUnique();

            builder.Property(p => p.DiscountPercent).IsRequired();
            builder.Property(p => p.MaxDiscountAmount).IsRequired();
            builder.Property(p => p.MinOrderValue).IsRequired();
            builder.Property(p => p.ExpiryDate).IsRequired();
            builder.Property(p => p.UsageLimit).IsRequired();
            builder.Property(p => p.TimesUsed).IsRequired().HasDefaultValue(0);
            builder.Property(p => p.IsActive).IsRequired().HasDefaultValue(true);

            // Seed data for testing
            builder.HasData(
                new PromoCode
                {
                    PromoCodeId = 1,
                    Code = "WELCOME50",
                    DiscountPercent = 50,
                    MaxDiscountAmount = 100,
                    MinOrderValue = 200,
                    ExpiryDate = new DateTime(2027, 12, 31, 0, 0, 0, DateTimeKind.Utc),
                    UsageLimit = 1000,
                    TimesUsed = 0,
                    IsActive = true
                },
                new PromoCode
                {
                    PromoCodeId = 2,
                    Code = "FLAT20",
                    DiscountPercent = 20,
                    MaxDiscountAmount = 50,
                    MinOrderValue = 0,
                    ExpiryDate = new DateTime(2027, 12, 31, 0, 0, 0, DateTimeKind.Utc),
                    UsageLimit = 5000,
                    TimesUsed = 0,
                    IsActive = true
                }
            );
        }
    }
}
