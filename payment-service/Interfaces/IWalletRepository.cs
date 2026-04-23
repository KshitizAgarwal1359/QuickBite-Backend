using QuickBite.Payment.Entities;

namespace QuickBite.Payment.Interfaces
{
    public interface IWalletRepository
    {
        Task<Wallet?> GetWalletByCustomerIdAsync(int customerId);
        Task<Wallet> AddWalletAsync(Wallet wallet);
        Task UpdateWalletAsync(Wallet wallet);
        Task AddStatementAsync(WalletStatement statement);
        Task<List<WalletStatement>> GetStatementsAsync(int walletId);
    }
}
