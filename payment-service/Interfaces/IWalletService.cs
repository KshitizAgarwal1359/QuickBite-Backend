using QuickBite.Payment.DTOs;

namespace QuickBite.Payment.Interfaces
{
    public interface IWalletService
    {
        Task<WalletResponseDto> GetBalanceAsync(int customerId);
        Task<WalletResponseDto> AddToWalletAsync(int customerId, WalletTopupRequestDto request);
        Task<WalletResponseDto> PayFromWalletAsync(int customerId, WalletPayRequestDto request);
        Task<List<WalletStatementResponseDto>> GetStatementsAsync(int customerId);
    }
}
