using System.ComponentModel.DataAnnotations;

namespace QuickBite.Restaurant.DTOs
{
    public class UpdateRatingRequestDto
    {
        [Required(ErrorMessage = "Average rating is required")]
        [Range(0, 5, ErrorMessage = "Rating must be between 0 and 5")]
        public double AvgRating { get; set; }
    }
}
