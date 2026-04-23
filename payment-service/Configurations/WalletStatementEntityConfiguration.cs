using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuickBite.Payment.Entities;

namespace QuickBite.Payment.Configurations
{
    public class WalletStatementEntityConfiguration : IEntityTypeConfiguration<WalletStatement>
    {
        public void Configure(EntityTypeBuilder<WalletStatement> builder)
        {
            builder.ToTable("WalletStatements");
            builder.HasKey(s => s.StatementId);
            builder.Property(s => s.StatementId).ValueGeneratedOnAdd();

            builder.Property(s => s.WalletId).IsRequired();
            builder.Property(s => s.Amount).IsRequired();
            
            builder.Property(s => s.Type).IsRequired().HasMaxLength(20);
            builder.Property(s => s.Description).IsRequired().HasMaxLength(200);
            builder.Property(s => s.CreatedAt).IsRequired();
        }
    }
}
