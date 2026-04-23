using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuickBite.Payment.DTOs;
using QuickBite.Payment.Interfaces;

namespace QuickBite.Payment.Controllers
{
    [ApiController]
    [Route("api/v1/wallet")]
    [Produces("application/json")]
    [Authorize(Roles = "CUSTOMER")] // Only customers have wallets
    public class WalletController : ControllerBase
    {
        private readonly IWalletService _walletService;

        public WalletController(IWalletService walletService)
        {
            _walletService = walletService;
        }

        /// <summary>
        /// Get the current wallet balance.
        /// </summary>
        [HttpGet("balance/{customerId}")]
        [ProducesResponseType(typeof(WalletResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetBalance(int customerId)
        {
            if (GetCurrentUserId() != customerId) return Forbid();
            var result = await _walletService.GetBalanceAsync(customerId);
            return Ok(result);
        }

        /// <summary>
        /// Top up wallet balance using an external payment mode.
        /// </summary>
        [HttpPost("add")]
        [ProducesResponseType(typeof(WalletResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> AddToWallet([FromBody] WalletTopupRequestDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var customerId = GetCurrentUserId();
            var result = await _walletService.AddToWalletAsync(customerId, request);
            return Ok(result);
        }

        /// <summary>
        /// Internal endpoint to pay for an order from wallet balance.
        /// </summary>
        [HttpPost("pay")]
        [ProducesResponseType(typeof(WalletResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> PayFromWallet([FromBody] WalletPayRequestDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var customerId = GetCurrentUserId();
            var result = await _walletService.PayFromWalletAsync(customerId, request);
            return Ok(result);
        }

        /// <summary>
        /// Get wallet statement history (deposits and debits).
        /// </summary>
        [HttpGet("statements/{customerId}")]
        [ProducesResponseType(typeof(List<WalletStatementResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetStatements(int customerId)
        {
            if (GetCurrentUserId() != customerId) return Forbid();
            var result = await _walletService.GetStatementsAsync(customerId);
            return Ok(result);
        }

        // ─── Helper ─────────────────────────────────────────────────────────

        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(claim) || !int.TryParse(claim, out var userId))
                throw new UnauthorizedAccessException("Invalid or missing user identity claim");
            return userId;
        }
    }
}
