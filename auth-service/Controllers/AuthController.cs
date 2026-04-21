using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuickBite.Auth.DTOs;
using QuickBite.Auth.Interfaces;

namespace QuickBite.Auth.Controllers
{
    [ApiController]
    [Route("api/v1/auth")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Register a new user account.
        /// </summary>
        /// <param name="request">Registration details</param>
        /// <returns>JWT token and user profile</returns>
        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.RegisterAsync(request);
            return StatusCode(StatusCodes.Status201Created, result);
        }

        /// <summary>
        /// Login with email and password.
        /// </summary>
        /// <param name="request">Login credentials</param>
        /// <returns>JWT token and user profile</returns>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.LoginAsync(request);
            return Ok(result);
        }

        /// <summary>
        /// Logout the current user (client should discard the token).
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult Logout()
        {
            // JWT is stateless — the client must discard the token.
            // Server-side token invalidation can be added later via a token blacklist in Redis.
            _logger.LogInformation("User UserId:{UserId} logged out", GetCurrentUserId());
            return Ok(new { message = "Logged out successfully" });
        }

        /// <summary>
        /// Get the current authenticated user's profile.
        /// </summary>
        [HttpGet("profile")]
        [Authorize]
        [ProducesResponseType(typeof(ProfileResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProfile()
        {
            var userId = GetCurrentUserId();
            var profile = await _authService.GetProfileAsync(userId);
            return Ok(profile);
        }

        /// <summary>
        /// Update the current authenticated user's profile.
        /// </summary>
        /// <param name="request">Fields to update</param>
        [HttpPut("profile")]
        [Authorize]
        [ProducesResponseType(typeof(ProfileResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            var updated = await _authService.UpdateProfileAsync(userId, request);
            return Ok(updated);
        }

        /// <summary>
        /// Change the current authenticated user's password.
        /// </summary>
        /// <param name="request">Current and new password</param>
        [HttpPut("password")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            await _authService.ChangePasswordAsync(userId, request);
            return Ok(new { message = "Password changed successfully" });
        }

        /// <summary>
        /// Deactivate (soft-delete) the current authenticated user's account.
        /// </summary>
        [HttpDelete("deactivate")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeactivateAccount()
        {
            var userId = GetCurrentUserId();
            await _authService.DeactivateAccountAsync(userId);
            return Ok(new { message = "Account deactivated successfully" });
        }

        // ─── Helpers ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Extracts the authenticated user's ID from the JWT claims.
        /// </summary>
        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(claim) || !int.TryParse(claim, out var userId))
                throw new UnauthorizedAccessException("Invalid or missing user identity claim");
            return userId;
        }
    }
}
