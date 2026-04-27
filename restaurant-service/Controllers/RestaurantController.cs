using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuickBite.Restaurant.DTOs;
using QuickBite.Restaurant.Interfaces;

namespace QuickBite.Restaurant.Controllers
{
    [ApiController]
    [Route("api/v1/restaurants")]
    [Produces("application/json")]
    public class RestaurantController : ControllerBase
    {
        private readonly IRestaurantService _restaurantService;
        private readonly ILogger<RestaurantController> _logger;

        public RestaurantController(IRestaurantService restaurantService, ILogger<RestaurantController> logger)
        {
            _restaurantService = restaurantService;
            _logger = logger;
        }

        /// <summary>
        /// Register a new restaurant (Owner only). Pending admin approval.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "OWNER")]
        [ProducesResponseType(typeof(RestaurantResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RegisterRestaurant([FromBody] RegisterRestaurantRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var ownerId = GetCurrentUserId();
            var result = await _restaurantService.RegisterRestaurantAsync(ownerId, request);
            return StatusCode(StatusCodes.Status201Created, result);
        }

        /// <summary>
        /// Get a restaurant by ID.
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(RestaurantResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _restaurantService.GetByIdAsync(id);
            return Ok(result);
        }

        /// <summary>
        /// Get all restaurants owned by a specific user (Owner/Admin).
        /// </summary>
        [HttpGet("owner/{ownerId}")]
        [Authorize(Roles = "OWNER,ADMIN")]
        [ProducesResponseType(typeof(List<RestaurantResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByOwner(int ownerId)
        {
            var result = await _restaurantService.GetByOwnerAsync(ownerId);
            return Ok(result);
        }

        /// <summary>
        /// Get all restaurants (Admin).
        /// </summary>
        [HttpGet("all")]
        [Authorize(Roles = "ADMIN")]
        [ProducesResponseType(typeof(List<RestaurantResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var result = await _restaurantService.GetAllAsync();
            return Ok(result);
        }

        /// <summary>
        /// Filter approved restaurants by cuisine type.
        /// </summary>
        [HttpGet("cuisine/{type}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<RestaurantResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByCuisine(string type)
        {
            var result = await _restaurantService.GetByCuisineAsync(type);
            return Ok(result);
        }

        /// <summary>
        /// Filter approved restaurants by city.
        /// </summary>
        [HttpGet("city/{city}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<RestaurantResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByCity(string city)
        {
            var result = await _restaurantService.GetByCityAsync(city);
            return Ok(result);
        }

        /// <summary>
        /// Find nearby approved and open restaurants using Haversine geo-proximity search.
        /// </summary>
        /// <param name="latitude">GPS latitude (-90 to 90)</param>
        /// <param name="longitude">GPS longitude (-180 to 180)</param>
        /// <param name="radiusInKm">Search radius in km (default: 5)</param>
        [HttpGet("nearby")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<RestaurantResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetNearby(
            [FromQuery] double latitude,
            [FromQuery] double longitude,
            [FromQuery] double radiusInKm = 5)
        {
            if (latitude < -90 || latitude > 90)
                return BadRequest(new { message = "Latitude must be between -90 and 90" });

            if (longitude < -180 || longitude > 180)
                return BadRequest(new { message = "Longitude must be between -180 and 180" });

            if (radiusInKm <= 0 || radiusInKm > 50)
                return BadRequest(new { message = "Radius must be between 0.1 and 50 km" });

            var result = await _restaurantService.GetNearbyAsync(latitude, longitude, radiusInKm);
            return Ok(result);
        }

        /// <summary>
        /// Search approved restaurants by keyword (name).
        /// </summary>
        [HttpGet("search")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<RestaurantResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Search([FromQuery] string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return BadRequest(new { message = "Search keyword is required" });

            var result = await _restaurantService.SearchRestaurantsAsync(keyword);
            return Ok(result);
        }

        /// <summary>
        /// Update a restaurant profile (Owner or Admin).
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "OWNER,ADMIN")]
        [ProducesResponseType(typeof(RestaurantResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateRestaurant(int id, [FromBody] UpdateRestaurantRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var callerId = GetCurrentUserId();
            var callerRole = GetCurrentUserRole();
            var result = await _restaurantService.UpdateRestaurantAsync(id, callerId, callerRole, request);
            return Ok(result);
        }

        /// <summary>
        /// Approve a restaurant (Admin only).
        /// </summary>
        [HttpPut("{id}/approve")]
        [Authorize(Roles = "ADMIN")]
        [ProducesResponseType(typeof(RestaurantResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ApproveRestaurant(int id)
        {
            var result = await _restaurantService.ApproveRestaurantAsync(id);
            return Ok(result);
        }

        /// <summary>
        /// Toggle restaurant open/closed status (Owner only).
        /// </summary>
        [HttpPut("{id}/toggleOpen")]
        [Authorize(Roles = "OWNER")]
        [ProducesResponseType(typeof(RestaurantResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ToggleOpen(int id)
        {
            var callerId = GetCurrentUserId();
            var result = await _restaurantService.ToggleIsOpenAsync(id, callerId);
            return Ok(result);
        }

        /// <summary>
        /// Delete a restaurant (Admin only).
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteRestaurant(int id)
        {
            await _restaurantService.DeleteRestaurantAsync(id);
            return Ok(new { message = "Restaurant deleted successfully" });
        }

        /// <summary>
        /// Update a restaurant's average rating (called by Review-Service).
        /// </summary>
        [HttpPut("{id}/rating")]
        [Authorize]
        [ProducesResponseType(typeof(RestaurantResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateRating(int id, [FromBody] UpdateRatingRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _restaurantService.UpdateRatingAsync(id, request.AvgRating);
            return Ok(result);
        }

        // ─── Helpers ──────────────────────────────────────────────────────────────

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
