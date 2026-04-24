using Microsoft.EntityFrameworkCore;
using QuickBite.Delivery.Configurations;
using QuickBite.Delivery.Entities;

namespace QuickBite.Delivery.Data
{
    public class DeliveryDbContext : DbContext
    {
        public DeliveryDbContext(DbContextOptions<DeliveryDbContext> options) : base(options) { }

        public DbSet<DeliveryAgent> DeliveryAgents { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new AgentEntityConfiguration());
        }
    }
}
