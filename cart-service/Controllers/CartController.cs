using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuickBite.Cart.DTOs;
using QuickBite.Cart.Interfaces;

namespace QuickBite.Cart.Controllers
{
    [ApiController]
    [Route("api/v1/cart")]
    [Produces("application/json")]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;
        private readonly ILogger<CartController> _logger;

        public CartController(ICartService cartService, ILogger<CartController> logger)
        {
            _cartService = cartService;
            _logger = logger;
        }

        /// <summary>
        /// Get a customer's active cart.
        /// </summary>
        [HttpGet("{customerId}")]
        [Authorize(Roles = "CUSTOMER")]
        [ProducesResponseType(typeof(CartResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCart(int customerId)
        {
            var callerId = GetCurrentUserId();
            if (callerId != customerId)
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "You can only view your own cart" });

            var result = await _cartService.GetCartByCustomerAsync(customerId);
            return Ok(result);
        }

        /// <summary>
        /// Add an item to cart. Creates cart if none exists. Enforces single-restaurant rule.
        /// </summary>
        [HttpPost("addItem")]
        [Authorize(Roles = "CUSTOMER")]
        [ProducesResponseType(typeof(CartResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> AddItem([FromBody] AddItemRequestDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var customerId = GetCurrentUserId();
            var result = await _cartService.AddItemAsync(customerId, request);
            return Ok(result);
        }

        /// <summary>
        /// Remove a specific item from the cart.
        /// </summary>
        [HttpDelete("removeItem/{cartId}/{itemId}")]
        [Authorize(Roles = "CUSTOMER")]
        [ProducesResponseType(typeof(CartResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveItem(int cartId, int itemId)
        {
            var customerId = GetCurrentUserId();
            var result = await _cartService.RemoveItemAsync(cartId, itemId, customerId);
            return Ok(result);
        }

        /// <summary>
        /// Update quantity of an item in the cart. Set to 0 to remove.
        /// </summary>
        [HttpPut("updateQty")]
        [Authorize(Roles = "CUSTOMER")]
        [ProducesResponseType(typeof(CartResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateQuantity([FromBody] UpdateQuantityRequestDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var customerId = GetCurrentUserId();
            var result = await _cartService.UpdateQuantityAsync(customerId, request);
            return Ok(result);
        }

        /// <summary>
        /// Clear the entire cart (deletes cart and all items).
        /// </summary>
        [HttpDelete("clear/{cartId}")]
        [Authorize(Roles = "CUSTOMER")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ClearCart(int cartId)
        {
            var customerId = GetCurrentUserId();
            await _cartService.ClearCartAsync(cartId, customerId);
            return Ok(new { message = "Cart cleared successfully" });
        }

        /// <summary>
        /// Apply a promo code to the cart.
        /// </summary>
        [HttpPost("applyPromo")]
        [Authorize(Roles = "CUSTOMER")]
        [ProducesResponseType(typeof(CartResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> ApplyPromo([FromBody] ApplyPromoRequestDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var customerId = GetCurrentUserId();
            var result = await _cartService.ApplyPromoCodeAsync(customerId, request);
            return Ok(result);
        }

        /// <summary>
        /// List all active carts (Admin debug/support).
        /// </summary>
        [HttpGet("all")]
        [Authorize(Roles = "ADMIN")]
        [ProducesResponseType(typeof(List<CartResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllCarts()
        {
            var result = await _cartService.GetAllCartsAsync();
            return Ok(result);
        }

        // ─── Helper ──────────────────────────────────────────────────────────────

        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(claim) || !int.TryParse(claim, out var userId))
                throw new UnauthorizedAccessException("Invalid or missing user identity claim");
            return userId;
        }
    }
}
