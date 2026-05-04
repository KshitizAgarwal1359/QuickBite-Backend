using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuickBite.Review.DTOs;
using QuickBite.Review.Interfaces;
using System.Security.Claims;

namespace QuickBite.Review.Controllers
{
    [ApiController]
    [Route("api/v1/reviews")]
    [Produces("application/json")]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        /// <summary>
        /// Submit a new dual review (food and delivery) for an order.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "CUSTOMER")]
        [ProducesResponseType(typeof(ReviewResponseDto), StatusCodes.Status201Created)]
        public async Task<IActionResult> AddReview([FromBody] CreateReviewDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var customerId = GetCurrentUserId();
            var result = await _reviewService.AddReviewAsync(customerId, request);
            return StatusCode(StatusCodes.Status201Created, result);
        }

        /// <summary>
        /// Get all reviews for a specific restaurant.
        /// </summary>
        [HttpGet("restaurant/{id}")]
        [ProducesResponseType(typeof(IEnumerable<ReviewResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByRestaurant(int id)
        {
            var result = await _reviewService.GetByRestaurantAsync(id);
            return Ok(result);
        }

        /// <summary>
        /// Get all reviews submitted by a specific customer.
        /// </summary>
        [HttpGet("customer/{id}")]
        [Authorize(Roles = "CUSTOMER,ADMIN")]
        [ProducesResponseType(typeof(IEnumerable<ReviewResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByCustomer(int id)
        {
            var userId = GetCurrentUserId();
            var role = GetCurrentUserRole();
            if (role == "CUSTOMER" && userId != id) return Forbid();
            
            var result = await _reviewService.GetByCustomerAsync(id);
            return Ok(result);
        }

        /// <summary>
        /// Get the review associated with a specific order.
        /// </summary>
        [HttpGet("order/{id}")]
        [Authorize(Roles = "CUSTOMER,ADMIN")]
        [ProducesResponseType(typeof(ReviewResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByOrder(int id)
        {
            var result = await _reviewService.GetByOrderAsync(id);
            var userId = GetCurrentUserId();
            var role = GetCurrentUserRole();
            if (role == "CUSTOMER" && result.CustomerId != userId) return Forbid();

            return Ok(result);
        }

        /// <summary>
        /// Get all delivery reviews for a specific delivery agent.
        /// </summary>
        [HttpGet("agent/{id}")]
        [ProducesResponseType(typeof(IEnumerable<ReviewResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByAgent(int id)
        {
            var result = await _reviewService.GetByAgentAsync(id);
            return Ok(result);
        }

        /// <summary>
        /// Get all reviews across the platform (Admin only).
        /// </summary>
        [HttpGet("all")]
        [Authorize(Roles = "ADMIN")]
        [ProducesResponseType(typeof(IEnumerable<ReviewResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllReviews()
        {
            var result = await _reviewService.GetAllReviewsAsync();
            return Ok(result);
        }

        /// <summary>
        /// Update an existing review.
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "CUSTOMER")]
        [ProducesResponseType(typeof(ReviewResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateReview(int id, [FromBody] UpdateReviewDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var customerId = GetCurrentUserId();
            var result = await _reviewService.UpdateReviewAsync(id, customerId, request);
            return Ok(result);
        }

        /// <summary>
        /// Delete a review (Admin only).
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteReview(int id)
        {
            await _reviewService.DeleteReviewAsync(id);
            return NoContent();
        }

        /// <summary>
        /// Get the computed average food rating for a restaurant.
        /// </summary>
        [HttpGet("avgFood/{restId}")]
        [ProducesResponseType(typeof(double), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAvgFoodRating(int restId)
        {
            var result = await _reviewService.GetAvgFoodRatingAsync(restId);
            return Ok(result);
        }

        /// <summary>
        /// Get the computed average delivery rating for an agent.
        /// </summary>
        [HttpGet("avgDelivery/{agentId}")]
        [ProducesResponseType(typeof(double), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAvgDeliveryRating(int agentId)
        {
            var result = await _reviewService.GetAvgDeliveryRatingAsync(agentId);
            return Ok(result);
        }

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
