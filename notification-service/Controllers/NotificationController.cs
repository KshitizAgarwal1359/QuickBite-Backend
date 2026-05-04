using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuickBite.Notification.DTOs;
using QuickBite.Notification.Interfaces;
using System.Security.Claims;

namespace QuickBite.Notification.Controllers
{
    [ApiController]
    [Route("api/v1/notifications")]
    [Produces("application/json")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        /// <summary>
        /// Get user notifications
        /// </summary>
        [HttpGet("{recipientId}")]
        [ProducesResponseType(typeof(IEnumerable<Entities.Notification>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUserNotifications(int recipientId)
        {
            var userId = GetCurrentUserId();
            var role = GetCurrentUserRole();
            if (role != "ADMIN" && userId != recipientId) return Forbid();

            var result = await _notificationService.GetByRecipient(recipientId);
            return Ok(result);
        }

        /// <summary>
        /// Mark as read
        /// </summary>
        [HttpPut("{id}/read")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            await _notificationService.MarkAsRead(id);
            return NoContent();
        }

        /// <summary>
        /// Mark all as read
        /// </summary>
        [HttpPut("readAll/{userId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> MarkAllAsRead(int userId)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId != userId && GetCurrentUserRole() != "ADMIN") return Forbid();

            await _notificationService.MarkAllRead(userId);
            return NoContent();
        }

        /// <summary>
        /// Get unread count
        /// </summary>
        [HttpGet("unread/{userId}")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUnreadCount(int userId)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId != userId && GetCurrentUserRole() != "ADMIN") return Forbid();

            var count = await _notificationService.GetUnreadCount(userId);
            return Ok(count);
        }

        /// <summary>
        /// Delete notification
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            await _notificationService.DeleteNotification(id);
            return NoContent();
        }

        /// <summary>
        /// Broadcast notification
        /// </summary>
        [HttpPost("bulk")]
        [Authorize(Roles = "ADMIN")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Broadcast([FromBody] SendBulkRequestDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            await _notificationService.SendBulk(request.RecipientIds, request.Title, request.Message);
            return Ok();
        }

        /// <summary>
        /// All platform notifications
        /// </summary>
        [HttpGet("all")]
        [Authorize(Roles = "ADMIN")]
        [ProducesResponseType(typeof(IEnumerable<Entities.Notification>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllNotifications()
        {
            var result = await _notificationService.GetAll();
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
