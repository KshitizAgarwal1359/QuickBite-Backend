using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuickBite.Notification.Entities
{
    public class Notification
    {
        [Key]
        public int NotificationId { get; set; }

        [Required]
        public int RecipientId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Type { get; set; } = null!; // ORDER / PAYMENT / PROMO / DELIVERY

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = null!;

        [Required]
        [MaxLength(2000)]
        public string Message { get; set; } = null!;

        [Required]
        [MaxLength(20)]
        public string Channel { get; set; } = null!; // APP / EMAIL / SMS

        public int? RelatedId { get; set; }

        [MaxLength(50)]
        public string? RelatedType { get; set; } // ORDER, PAYMENT, etc.

        public bool IsRead { get; set; } = false;

        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }
}
