using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuickBite.Cart.Entities;

namespace QuickBite.Cart.Configurations
{
    public class CartItemEntityConfiguration : IEntityTypeConfiguration<CartItem>
    {
        public void Configure(EntityTypeBuilder<CartItem> builder)
        {
            builder.ToTable("CartItems");

            builder.HasKey(i => i.ItemId);
            builder.Property(i => i.ItemId).ValueGeneratedOnAdd();

            builder.Property(i => i.CartId).IsRequired();
            builder.Property(i => i.MenuItemId).IsRequired();
            builder.HasIndex(i => i.MenuItemId);

            builder.Property(i => i.Name).IsRequired().HasMaxLength(100);
            builder.Property(i => i.Price).IsRequired();
            builder.Property(i => i.Quantity).IsRequired().HasDefaultValue(1);
            builder.Property(i => i.Customization).HasMaxLength(256);
        }
    }
}
