using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuickBite.Menu.Entities;

namespace QuickBite.Menu.Configurations
{
    public class MenuCategoryEntityConfiguration : IEntityTypeConfiguration<MenuCategory>
    {
        public void Configure(EntityTypeBuilder<MenuCategory> builder)
        {
            builder.ToTable("MenuCategories");

            builder.HasKey(c => c.CategoryId);
            builder.Property(c => c.CategoryId).ValueGeneratedOnAdd();

            builder.Property(c => c.RestaurantId).IsRequired();
            builder.HasIndex(c => c.RestaurantId);

            builder.Property(c => c.Name).IsRequired().HasMaxLength(100);
            builder.Property(c => c.Description).HasMaxLength(300);
            builder.Property(c => c.ImageUrl).HasMaxLength(500);
            builder.Property(c => c.DisplayOrder).IsRequired().HasDefaultValue(0);

            // One category has many items
            builder.HasMany(c => c.Items)
                .WithOne(i => i.Category)
                .HasForeignKey(i => i.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
