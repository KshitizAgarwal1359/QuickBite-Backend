using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuickBite.Delivery.DTOs;
using QuickBite.Delivery.Interfaces;

namespace QuickBite.Delivery.Controllers
{
    [ApiController]
    [Route("api/v1/agents")]
    [Produces("application/json")]
    public class AgentController : ControllerBase
    {
        private readonly IDeliveryService _deliveryService;

        public AgentController(IDeliveryService deliveryService)
        {
            _deliveryService = deliveryService;
        }

        /// <summary>
        /// Register a new delivery agent.
        /// </summary>
        [HttpPost("register")]
        [Authorize(Roles = "AGENT")]
        [ProducesResponseType(typeof(AgentResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> RegisterAgent([FromBody] AgentRegistrationRequestDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var userId = GetCurrentUserId();
            var result = await _deliveryService.RegisterAgentAsync(userId, request);
            return Ok(result);
        }

        /// <summary>
        /// Get an agent by their ID.
        /// </summary>
        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(AgentResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAgent(int id)
        {
            var result = await _deliveryService.GetAgentByIdAsync(id);
            return Ok(result);
        }

        /// <summary>
        /// Get all agents.
        /// </summary>
        [HttpGet("all")]
        [Authorize(Roles = "ADMIN")]
        [ProducesResponseType(typeof(List<AgentResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllAgents()
        {
            var result = await _deliveryService.GetAllAgentsAsync();
            return Ok(result);
        }

        /// <summary>
        /// Find nearby available agents using coordinates.
        /// </summary>
        [HttpGet("nearby")]
        [Authorize(Roles = "SYSTEM,ADMIN")]
        [ProducesResponseType(typeof(List<AgentDistanceResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetNearbyAgents([FromQuery] double latitude, [FromQuery] double longitude, [FromQuery] double radiusInKm = 5)
        {
            var result = await _deliveryService.GetNearbyAgentsAsync(latitude, longitude, radiusInKm);
            return Ok(result);
        }

        /// <summary>
        /// Update live GPS location of the agent.
        /// </summary>
        [HttpPut("{id}/location")]
        [Authorize(Roles = "AGENT")]
        [ProducesResponseType(typeof(AgentResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateLocation(int id, [FromBody] LocationUpdateRequestDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var userId = GetCurrentUserId();
            var agent = await _deliveryService.GetAgentByIdAsync(id);
            if (agent.UserId != userId) return Forbid("You can only update your own location.");

            var result = await _deliveryService.UpdateLocationAsync(userId, request);
            return Ok(result);
        }

        /// <summary>
        /// Toggle agent's online/offline availability.
        /// </summary>
        [HttpPut("{id}/availability")]
        [Authorize(Roles = "AGENT")]
        [ProducesResponseType(typeof(AgentResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> SetAvailability(int id, [FromQuery] bool isAvailable)
        {
            var userId = GetCurrentUserId();
            var agent = await _deliveryService.GetAgentByIdAsync(id);
            if (agent.UserId != userId) return Forbid("You can only change your own availability.");

            var result = await _deliveryService.SetAvailabilityAsync(userId, isAvailable);
            return Ok(result);
        }

        /// <summary>
        /// Admin verification for a new agent.
        /// </summary>
        [HttpPut("{id}/verify")]
        [Authorize(Roles = "ADMIN")]
        [ProducesResponseType(typeof(AgentResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> VerifyAgent(int id)
        {
            var result = await _deliveryService.VerifyAgentAsync(id);
            return Ok(result);
        }

        /// <summary>
        /// Update aggregate rating based on customer feedback.
        /// </summary>
        [HttpPut("{id}/rating")]
        [Authorize(Roles = "SYSTEM,ADMIN")]
        [ProducesResponseType(typeof(AgentResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateRating(int id, [FromBody] AgentRatingRequestDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _deliveryService.UpdateRatingAsync(id, request.Rating);
            return Ok(result);
        }

        /// <summary>
        /// System assigns an order to an agent.
        /// </summary>
        [HttpPost("{id}/assignOrder")]
        [Authorize(Roles = "SYSTEM,ADMIN")]
        [ProducesResponseType(typeof(AgentResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> AssignOrder(int id, [FromBody] AssignOrderRequestDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _deliveryService.AssignOrderAsync(id, request.OrderId);
            return Ok(result);
        }

        /// <summary>
        /// Agent marks an order as delivered.
        /// </summary>
        [HttpPost("{id}/completeDelivery")]
        [Authorize(Roles = "AGENT")]
        [ProducesResponseType(typeof(AgentResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> CompleteDelivery(int id, [FromBody] AssignOrderRequestDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var userId = GetCurrentUserId();
            var agent = await _deliveryService.GetAgentByIdAsync(id);
            if (agent.UserId != userId) return Forbid();

            var result = await _deliveryService.CompleteDeliveryAsync(userId, request.OrderId);
            return Ok(result);
        }

        /// <summary>
        /// Get active deliveries assigned to the agent.
        /// </summary>
        [HttpGet("{id}/activeDeliveries")]
        [Authorize(Roles = "AGENT")]
        [ProducesResponseType(typeof(List<int>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetActiveDeliveries(int id)
        {
            var userId = GetCurrentUserId();
            var agent = await _deliveryService.GetAgentByIdAsync(id);
            if (agent.UserId != userId) return Forbid();

            var result = await _deliveryService.GetActiveDeliveriesAsync(userId);
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
