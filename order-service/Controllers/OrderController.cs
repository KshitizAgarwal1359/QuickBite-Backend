using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuickBite.Order.DTOs;
using QuickBite.Order.Interfaces;

namespace QuickBite.Order.Controllers
{
    [ApiController]
    [Route("api/v1/orders")]
    [Produces("application/json")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        /// <summary>
        /// Place a new order from a confirmed cart.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "CUSTOMER")]
        [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status201Created)]
        public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderRequestDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var customerId = GetCurrentUserId();
            var result = await _orderService.PlaceOrderAsync(customerId, request);
            return StatusCode(StatusCodes.Status201Created, result);
        }

        /// <summary>
        /// Get an order by its ID.
        /// </summary>
        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetOrder(int id)
        {
            var userId = GetCurrentUserId();
            var role = GetCurrentUserRole();
            var result = await _orderService.GetOrderByIdAsync(id, userId, role);
            return Ok(result);
        }

        /// <summary>
        /// Get order history for a specific customer.
        /// </summary>
        [HttpGet("customer/{id}")]
        [Authorize(Roles = "CUSTOMER")]
        [ProducesResponseType(typeof(List<OrderResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCustomerOrders(int id)
        {
            var userId = GetCurrentUserId();
            if (userId != id) return Forbid("You can only view your own orders");
            var result = await _orderService.GetOrdersByCustomerAsync(id);
            return Ok(result);
        }

        /// <summary>
        /// Get all orders for a specific restaurant.
        /// </summary>
        [HttpGet("restaurant/{id}")]
        [Authorize(Roles = "OWNER,ADMIN")]
        [ProducesResponseType(typeof(List<OrderResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRestaurantOrders(int id)
        {
            var result = await _orderService.GetOrdersByRestaurantAsync(id);
            return Ok(result);
        }

        /// <summary>
        /// Get all platform-wide active orders.
        /// </summary>
        [HttpGet("active")]
        [Authorize(Roles = "ADMIN")]
        [ProducesResponseType(typeof(List<OrderResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetActiveOrders()
        {
            var result = await _orderService.GetActiveOrdersAsync();
            return Ok(result);
        }

        /// <summary>
        /// Update the lifecycle status of an order.
        /// </summary>
        [HttpPut("{id}/status")]
        [Authorize(Roles = "OWNER,AGENT,ADMIN")]
        [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusRequestDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _orderService.UpdateOrderStatusAsync(id, request.OrderStatus);
            return Ok(result);
        }

        /// <summary>
        /// Assign a delivery agent to an order.
        /// </summary>
        [HttpPut("{id}/assignAgent")]
        [Authorize(Roles = "SYSTEM,ADMIN")]
        [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> AssignAgent(int id, [FromBody] AssignAgentRequestDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _orderService.AssignDeliveryAgentAsync(id, request.DeliveryAgentId);
            return Ok(result);
        }

        /// <summary>
        /// Cancel an order before preparation begins.
        /// </summary>
        [HttpPut("{id}/cancel")]
        [Authorize(Roles = "CUSTOMER,ADMIN")]
        [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var userId = GetCurrentUserId();
            var result = await _orderService.CancelOrderAsync(id, userId);
            return Ok(result);
        }

        /// <summary>
        /// Recreate a past order's cart configuration.
        /// </summary>
        [HttpPost("{id}/reorder")]
        [Authorize(Roles = "CUSTOMER")]
        [ProducesResponseType(typeof(PlaceOrderRequestDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> Reorder(int id)
        {
            var userId = GetCurrentUserId();
            var result = await _orderService.ReorderFromHistoryAsync(id, userId);
            return Ok(result);
        }

        /// <summary>
        /// Get the total order count for a specific restaurant.
        /// </summary>
        [HttpGet("count/{restId}")]
        [Authorize(Roles = "OWNER,ADMIN")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetOrderCount(int restId)
        {
            var result = await _orderService.GetOrderCountForRestaurantAsync(restId);
            return Ok(result);
        }

        /// <summary>
        /// Get active orders assigned to a delivery agent.
        /// </summary>
        [HttpGet("agent/{agentId}")]
        [Authorize(Roles = "AGENT,ADMIN")]
        [ProducesResponseType(typeof(List<OrderResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAgentOrders(int agentId)
        {
            var result = await _orderService.GetOrdersByAgentAsync(agentId);
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
