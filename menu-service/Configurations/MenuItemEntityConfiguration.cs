using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuickBite.Menu.Entities;

namespace QuickBite.Menu.Configurations
{
    public class MenuItemEntityConfiguration : IEntityTypeConfiguration<MenuItem>
    {
        public void Configure(EntityTypeBuilder<MenuItem> builder)
        {
            builder.ToTable("MenuItems");

            builder.HasKey(i => i.ItemId);
            builder.Property(i => i.ItemId).ValueGeneratedOnAdd();

            builder.Property(i => i.RestaurantId).IsRequired();
            builder.HasIndex(i => i.RestaurantId);

            builder.Property(i => i.CategoryId).IsRequired();
            builder.HasIndex(i => i.CategoryId);

            builder.Property(i => i.Name).IsRequired().HasMaxLength(100);
            builder.Property(i => i.Description).HasMaxLength(500);
            builder.Property(i => i.Price).IsRequired();
            builder.Property(i => i.DiscountedPrice).IsRequired();
            builder.Property(i => i.ImageUrl).HasMaxLength(500);

            builder.Property(i => i.IsVeg).IsRequired();
            builder.HasIndex(i => i.IsVeg);

            builder.Property(i => i.IsAvailable).IsRequired().HasDefaultValue(true);
            builder.Property(i => i.Rating).IsRequired().HasDefaultValue(0);
            builder.Property(i => i.Calories).IsRequired();
            builder.Property(i => i.Tags).HasMaxLength(256);
        }
    }
}
