using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace QuickBite.Cart.Configurations
{
    public class CartEntityConfiguration : IEntityTypeConfiguration<Entities.Cart>
    {
        public void Configure(EntityTypeBuilder<Entities.Cart> builder)
        {
            builder.ToTable("Carts");

            builder.HasKey(c => c.CartId);
            builder.Property(c => c.CartId).ValueGeneratedOnAdd();

            builder.Property(c => c.CustomerId).IsRequired();
            builder.HasIndex(c => c.CustomerId).IsUnique();

            builder.Property(c => c.RestaurantId).IsRequired();
            builder.HasIndex(c => c.RestaurantId);

            builder.Property(c => c.TotalPrice).IsRequired().HasDefaultValue(0);
            builder.Property(c => c.DiscountAmount).IsRequired().HasDefaultValue(0);
            builder.Property(c => c.PromoCode).HasMaxLength(50);
            builder.Property(c => c.CreatedAt).IsRequired();

            builder.HasMany(c => c.Items)
                .WithOne(i => i.Cart)
                .HasForeignKey(i => i.CartId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
