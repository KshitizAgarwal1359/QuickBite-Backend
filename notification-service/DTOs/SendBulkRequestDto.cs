using System.ComponentModel.DataAnnotations;

namespace QuickBite.Notification.DTOs
{
    public class SendBulkRequestDto
    {
        [Required]
        public List<int> RecipientIds { get; set; } = new();

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(2000)]
        public string Message { get; set; } = string.Empty;
    }
}
