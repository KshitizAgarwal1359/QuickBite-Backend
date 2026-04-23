using Microsoft.EntityFrameworkCore;
using QuickBite.Payment.Data;
using QuickBite.Payment.Entities;
using QuickBite.Payment.Interfaces;

namespace QuickBite.Payment.Repository
{
    public class WalletRepository : IWalletRepository
    {
        private readonly PaymentDbContext _context;

        public WalletRepository(PaymentDbContext context) { _context = context; }

        public async Task<Wallet?> GetWalletByCustomerIdAsync(int customerId)
        {
            return await _context.Wallets
                .Include(w => w.Statements)
                .FirstOrDefaultAsync(w => w.CustomerId == customerId);
        }

        public async Task<Wallet> AddWalletAsync(Wallet wallet)
        {
            await _context.Wallets.AddAsync(wallet);
            await _context.SaveChangesAsync();
            return wallet;
        }

        public async Task UpdateWalletAsync(Wallet wallet)
        {
            _context.Wallets.Update(wallet);
            await _context.SaveChangesAsync();
        }

        public async Task AddStatementAsync(WalletStatement statement)
        {
            await _context.WalletStatements.AddAsync(statement);
            await _context.SaveChangesAsync();
        }

        public async Task<List<WalletStatement>> GetStatementsAsync(int walletId)
        {
            return await _context.WalletStatements
                .Where(s => s.WalletId == walletId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }
    }
}
