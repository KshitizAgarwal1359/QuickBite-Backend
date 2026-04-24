using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuickBite.Delivery.Entities;

namespace QuickBite.Delivery.Configurations
{
    public class AgentEntityConfiguration : IEntityTypeConfiguration<DeliveryAgent>
    {
        public void Configure(EntityTypeBuilder<DeliveryAgent> builder)
        {
            builder.ToTable("DeliveryAgents");
            builder.HasKey(a => a.AgentId);
            builder.Property(a => a.AgentId).ValueGeneratedOnAdd();

            builder.Property(a => a.UserId).IsRequired();
            builder.HasIndex(a => a.UserId).IsUnique();

            builder.Property(a => a.FullName).IsRequired().HasMaxLength(100);
            builder.Property(a => a.Phone).IsRequired().HasMaxLength(20);
            builder.Property(a => a.VehicleType).IsRequired().HasMaxLength(20);
            builder.Property(a => a.VehicleNumber).IsRequired().HasMaxLength(50);

            // Set default coordinates (e.g., 0,0) or nullable
            builder.Property(a => a.CurrentLatitude).HasDefaultValue(0);
            builder.Property(a => a.CurrentLongitude).HasDefaultValue(0);

            builder.Property(a => a.IsAvailable).HasDefaultValue(false);
            builder.Property(a => a.IsVerified).HasDefaultValue(false);
            
            builder.Property(a => a.AvgRating).HasDefaultValue(0.0);
            builder.Property(a => a.TotalDeliveries).HasDefaultValue(0);

            // Indexes for geo-queries and filtering
            builder.HasIndex(a => new { a.IsAvailable, a.IsVerified });
        }
    }
}
