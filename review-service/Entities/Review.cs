using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuickBite.Review.Entities
{
    public class Review
    {
        [Key]
        public int ReviewId { get; set; }
        
        [Required]
        public int OrderId { get; set; }
        
        [Required]
        public int CustomerId { get; set; }
        
        [Required]
        public int RestaurantId { get; set; }
        
        public int? AgentId { get; set; }
        
        [Range(1, 5)]
        public int FoodRating { get; set; }
        
        [Range(1, 5)]
        public int DeliveryRating { get; set; }
        
        [MaxLength(1000)]
        public string? Comment { get; set; }
        
        public DateTime ReviewDate { get; set; } = DateTime.UtcNow;
        
        public bool IsVerifiedOnly { get; set; } = false;
    }
}
