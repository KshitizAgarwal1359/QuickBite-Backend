using Microsoft.EntityFrameworkCore;
using QuickBite.Payment.Data;
using QuickBite.Payment.DTOs;
using QuickBite.Payment.Entities;
using QuickBite.Payment.Interfaces;

namespace QuickBite.Payment.Services
{
    public class WalletServiceImpl : IWalletService
    {
        private readonly IWalletRepository _walletRepo;
        private readonly PaymentDbContext _dbContext;
        private readonly ILogger<WalletServiceImpl> _logger;

        public WalletServiceImpl(IWalletRepository walletRepo, PaymentDbContext dbContext, ILogger<WalletServiceImpl> logger)
        {
            _walletRepo = walletRepo;
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<WalletResponseDto> GetBalanceAsync(int customerId)
        {
            var wallet = await GetOrCreateWalletAsync(customerId);
            return new WalletResponseDto { WalletId = wallet.WalletId, CustomerId = wallet.CustomerId, Balance = wallet.Balance };
        }

        public async Task<WalletResponseDto> AddToWalletAsync(int customerId, WalletTopupRequestDto request)
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    var wallet = await GetOrCreateWalletAsync(customerId);
                    
                    wallet.Balance += request.Amount;
                    await _walletRepo.UpdateWalletAsync(wallet);

                    var statement = new WalletStatement
                    {
                        WalletId = wallet.WalletId,
                        Amount = request.Amount,
                        Type = "DEPOSIT",
                        Description = $"Top-up via {request.RazorpayPaymentId}"
                    };
                    await _walletRepo.AddStatementAsync(statement);

                    await transaction.CommitAsync();
                    _logger.LogInformation("₹{Amount} added to Wallet {WalletId}. New Balance: ₹{Balance}", request.Amount, wallet.WalletId, wallet.Balance);
                    
                    return new WalletResponseDto { WalletId = wallet.WalletId, CustomerId = wallet.CustomerId, Balance = wallet.Balance };
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Failed to top up wallet for customer {CustomerId}", customerId);
                    throw;
                }
            });
        }

        public async Task<WalletResponseDto> PayFromWalletAsync(int customerId, WalletPayRequestDto request)
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    var wallet = await GetOrCreateWalletAsync(customerId);

                    if (wallet.Balance < request.Amount)
                    {
                        throw new InvalidOperationException($"Insufficient wallet balance. Available: ₹{wallet.Balance}, Required: ₹{request.Amount}");
                    }

                    wallet.Balance -= request.Amount;
                    await _walletRepo.UpdateWalletAsync(wallet);

                    var statement = new WalletStatement
                    {
                        WalletId = wallet.WalletId,
                        Amount = request.Amount,
                        Type = "DEBIT",
                        Description = $"Payment for Order {request.OrderId}"
                    };
                    await _walletRepo.AddStatementAsync(statement);

                    await transaction.CommitAsync();
                    _logger.LogInformation("₹{Amount} deducted from Wallet {WalletId} for Order {OrderId}. New Balance: ₹{Balance}", 
                        request.Amount, wallet.WalletId, request.OrderId, wallet.Balance);
                    
                    return new WalletResponseDto { WalletId = wallet.WalletId, CustomerId = wallet.CustomerId, Balance = wallet.Balance };
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Failed to pay from wallet for customer {CustomerId}, order {OrderId}", customerId, request.OrderId);
                    throw;
                }
            });
        }

        public async Task<List<WalletStatementResponseDto>> GetStatementsAsync(int customerId)
        {
            var wallet = await GetOrCreateWalletAsync(customerId);
            var statements = await _walletRepo.GetStatementsAsync(wallet.WalletId);

            return statements.Select(s => new WalletStatementResponseDto
            {
                StatementId = s.StatementId,
                Amount = s.Amount,
                Type = s.Type,
                Description = s.Description,
                CreatedAt = s.CreatedAt
            }).ToList();
        }

        private async Task<Wallet> GetOrCreateWalletAsync(int customerId)
        {
            var wallet = await _walletRepo.GetWalletByCustomerIdAsync(customerId);
            if (wallet == null)
            {
                wallet = new Wallet { CustomerId = customerId, Balance = 0 };
                wallet = await _walletRepo.AddWalletAsync(wallet);
                _logger.LogInformation("Created new wallet for Customer {CustomerId}", customerId);
            }
            return wallet;
        }
    }
}
