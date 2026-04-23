using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuickBite.Payment.DTOs;
using QuickBite.Payment.Interfaces;

namespace QuickBite.Payment.Controllers
{
    [ApiController]
    [Route("api/v1/payments")]
    [Produces("application/json")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        /// <summary>
        /// Process order payment. Handles COD, Wallet, and external gateways (Razorpay).
        /// </summary>
        [HttpPost("process")]
        [Authorize(Roles = "CUSTOMER,SYSTEM")]
        [ProducesResponseType(typeof(PaymentResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> ProcessPayment([FromBody] ProcessPaymentRequestDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var customerId = GetCurrentUserId();
            var result = await _paymentService.ProcessPaymentAsync(customerId, request);
            return Ok(result);
        }

        /// <summary>
        /// Get payment details for a specific order.
        /// </summary>
        [HttpGet("order/{orderId}")]
        [Authorize]
        [ProducesResponseType(typeof(PaymentResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPaymentByOrder(int orderId)
        {
            var userId = GetCurrentUserId();
            var role = GetCurrentUserRole();
            var result = await _paymentService.GetByOrderAsync(orderId, userId, role);
            return Ok(result);
        }

        /// <summary>
        /// Get all payments made by a customer.
        /// </summary>
        [HttpGet("customer/{customerId}")]
        [Authorize(Roles = "CUSTOMER,ADMIN")]
        [ProducesResponseType(typeof(List<PaymentResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCustomerPayments(int customerId)
        {
            var userId = GetCurrentUserId();
            var role = GetCurrentUserRole();
            
            if (role == "CUSTOMER" && userId != customerId)
                return Forbid("You can only view your own payments");

            var result = await _paymentService.GetByCustomerAsync(customerId);
            return Ok(result);
        }

        /// <summary>
        /// Initiate a refund for a payment. (Called by Order-Service on cancellation)
        /// </summary>
        [HttpPost("refund/{paymentId}")]
        [Authorize(Roles = "ADMIN,SYSTEM")]
        [ProducesResponseType(typeof(PaymentResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> RefundPayment(int paymentId)
        {
            var result = await _paymentService.RefundPaymentAsync(paymentId);
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

        private string GetCurrentUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        }
    }
}
