using Microsoft.EntityFrameworkCore;
using QuickBite.Restaurant.Configurations;

namespace QuickBite.Restaurant.Data
{
    public class RestaurantDbContext : DbContext
    {
        public RestaurantDbContext(DbContextOptions<RestaurantDbContext> options) : base(options)
        {
        }

        public DbSet<Entities.Restaurant> Restaurants { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new RestaurantEntityConfiguration());
        }
    }
}
