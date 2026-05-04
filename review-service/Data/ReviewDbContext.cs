using Microsoft.EntityFrameworkCore;

namespace QuickBite.Review.Data
{
    public class ReviewDbContext : DbContext
    {
        public ReviewDbContext(DbContextOptions<ReviewDbContext> options) : base(options) { }

        public DbSet<Entities.Review> Reviews { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<Entities.Review>()
                .HasIndex(r => r.OrderId)
                .IsUnique();
        }
    }
}
