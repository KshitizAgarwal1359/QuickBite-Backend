using System.ComponentModel.DataAnnotations;

namespace QuickBite.Restaurant.DTOs
{
    public class RegisterRestaurantRequestDto
    {
        [Required(ErrorMessage = "Restaurant name is required")]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Cuisine type is required")]
        [StringLength(50)]
        public string Cuisine { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required")]
        [StringLength(256)]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required")]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Latitude is required")]
        [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90")]
        public double Latitude { get; set; }

        [Required(ErrorMessage = "Longitude is required")]
        [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180")]
        public double Longitude { get; set; }

        [Phone(ErrorMessage = "Invalid phone number")]
        [StringLength(15)]
        public string? Phone { get; set; }

        [StringLength(2048)]
        public string? ImageUrl { get; set; }

        [Required(ErrorMessage = "Delivery radius is required")]
        [Range(0.5, 50, ErrorMessage = "Delivery radius must be between 0.5 and 50 km")]
        public double DeliveryRadius { get; set; }

        [Required(ErrorMessage = "Minimum order amount is required")]
        [Range(0, 10000)]
        public double MinOrderAmount { get; set; }

        [Required(ErrorMessage = "Estimated delivery time is required")]
        [Range(5, 120, ErrorMessage = "Estimated delivery must be between 5 and 120 minutes")]
        public int EstimatedDeliveryMin { get; set; }
    }
}
