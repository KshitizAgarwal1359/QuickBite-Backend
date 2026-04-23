using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace QuickBite.Order.Configurations
{
    public class OrderEntityConfiguration : IEntityTypeConfiguration<Entities.Order>
    {
        public void Configure(EntityTypeBuilder<Entities.Order> builder)
        {
            builder.ToTable("Orders");

            builder.HasKey(o => o.OrderId);
            builder.Property(o => o.OrderId).ValueGeneratedOnAdd();

            builder.Property(o => o.CustomerId).IsRequired();
            builder.HasIndex(o => o.CustomerId);

            builder.Property(o => o.RestaurantId).IsRequired();
            builder.HasIndex(o => o.RestaurantId);

            builder.Property(o => o.DeliveryAgentId);
            builder.HasIndex(o => o.DeliveryAgentId);

            builder.Property(o => o.TotalAmount).IsRequired();
            builder.Property(o => o.Discount).IsRequired().HasDefaultValue(0);
            builder.Property(o => o.FinalAmount).IsRequired();
            builder.Property(o => o.ModeOfPayment).IsRequired().HasMaxLength(20);
            
            builder.Property(o => o.OrderStatus).IsRequired().HasMaxLength(20);
            builder.HasIndex(o => o.OrderStatus);

            builder.Property(o => o.OrderDate).IsRequired();
            builder.Property(o => o.EstimatedDelivery);
            builder.Property(o => o.DeliveryAddress).IsRequired().HasMaxLength(500);
            builder.Property(o => o.SpecialInstructions).HasMaxLength(500);

            builder.HasMany(o => o.Items)
                .WithOne(i => i.Order)
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
