using System.ComponentModel.DataAnnotations;

namespace QuickBite.Review.DTOs
{
    public class UpdateReviewDto
    {
        [Range(1, 5)]
        public int FoodRating { get; set; }
        
        [Range(1, 5)]
        public int DeliveryRating { get; set; }
        
        [MaxLength(1000)]
        public string? Comment { get; set; }
    }
}
