using Microsoft.EntityFrameworkCore;
using QuickBite.Menu.Configurations;
using QuickBite.Menu.Entities;

namespace QuickBite.Menu.Data
{
    public class MenuDbContext : DbContext
    {
        public MenuDbContext(DbContextOptions<MenuDbContext> options) : base(options)
        {
        }

        public DbSet<MenuCategory> MenuCategories { get; set; }
        public DbSet<MenuItem> MenuItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new MenuCategoryEntityConfiguration());
            modelBuilder.ApplyConfiguration(new MenuItemEntityConfiguration());
        }
    }
}
