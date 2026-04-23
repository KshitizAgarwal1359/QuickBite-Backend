using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuickBite.Order.Entities;

namespace QuickBite.Order.Configurations
{
    public class OrderItemEntityConfiguration : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.ToTable("OrderItems");

            builder.HasKey(i => i.OrderItemId);
            builder.Property(i => i.OrderItemId).ValueGeneratedOnAdd();

            builder.Property(i => i.OrderId).IsRequired();
            builder.Property(i => i.MenuItemId).IsRequired();

            builder.Property(i => i.Name).IsRequired().HasMaxLength(100);
            builder.Property(i => i.Price).IsRequired();
            builder.Property(i => i.Quantity).IsRequired().HasDefaultValue(1);
            builder.Property(i => i.Customization).HasMaxLength(256);
        }
    }
}
