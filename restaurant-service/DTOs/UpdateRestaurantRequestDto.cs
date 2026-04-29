using System.ComponentModel.DataAnnotations;

namespace QuickBite.Restaurant.DTOs
{
    public class UpdateRestaurantRequestDto
    {
        [StringLength(100, MinimumLength = 2)]
        public string? Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(50)]
        public string? Cuisine { get; set; }

        [StringLength(256)]
        public string? Address { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [Range(-90, 90)]
        public double? Latitude { get; set; }

        [Range(-180, 180)]
        public double? Longitude { get; set; }

        [Phone]
        [StringLength(15)]
        public string? Phone { get; set; }

        [StringLength(2048)]
        public string? ImageUrl { get; set; }

        [Range(0.5, 50)]
        public double? DeliveryRadius { get; set; }

        [Range(0, 10000)]
        public double? MinOrderAmount { get; set; }

        [Range(5, 120)]
        public int? EstimatedDeliveryMin { get; set; }
    }
}
