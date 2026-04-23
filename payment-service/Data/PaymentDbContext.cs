using Microsoft.EntityFrameworkCore;
using QuickBite.Payment.Configurations;
using QuickBite.Payment.Entities;

namespace QuickBite.Payment.Data
{
    public class PaymentDbContext : DbContext
    {
        public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options) { }

        public DbSet<Entities.Payment> Payments { get; set; }
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<WalletStatement> WalletStatements { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new PaymentEntityConfiguration());
            modelBuilder.ApplyConfiguration(new WalletEntityConfiguration());
            modelBuilder.ApplyConfiguration(new WalletStatementEntityConfiguration());
        }
    }
}
