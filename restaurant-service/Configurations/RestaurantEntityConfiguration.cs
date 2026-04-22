using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace QuickBite.Restaurant.Configurations
{
    public class RestaurantEntityConfiguration : IEntityTypeConfiguration<Entities.Restaurant>
    {
        public void Configure(EntityTypeBuilder<Entities.Restaurant> builder)
        {
            builder.ToTable("Restaurants");

            builder.HasKey(r => r.RestaurantId);

            builder.Property(r => r.RestaurantId)
                .ValueGeneratedOnAdd();

            builder.Property(r => r.OwnerId)
                .IsRequired();

            builder.HasIndex(r => r.OwnerId);

            builder.Property(r => r.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(r => r.Description)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(r => r.Cuisine)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasIndex(r => r.Cuisine);

            builder.Property(r => r.Address)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(r => r.City)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasIndex(r => r.City);

            builder.Property(r => r.Latitude)
                .IsRequired();

            builder.Property(r => r.Longitude)
                .IsRequired();

            builder.Property(r => r.Phone)
                .HasMaxLength(15);

            builder.Property(r => r.AvgRating)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(r => r.IsOpen)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(r => r.IsApproved)
                .IsRequired()
                .HasDefaultValue(false);

            // Composite index for listing: approved + open restaurants
            builder.HasIndex(r => new { r.IsApproved, r.IsOpen });

            builder.Property(r => r.DeliveryRadius)
                .IsRequired();

            builder.Property(r => r.MinOrderAmount)
                .IsRequired();

            builder.Property(r => r.EstimatedDeliveryMin)
                .IsRequired();
        }
    }
}
