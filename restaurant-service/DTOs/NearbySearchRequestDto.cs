using System.ComponentModel.DataAnnotations;

namespace QuickBite.Restaurant.DTOs
{
    public class NearbySearchRequestDto
    {
        [Required(ErrorMessage = "Latitude is required")]
        [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90")]
        public double Latitude { get; set; }

        [Required(ErrorMessage = "Longitude is required")]
        [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180")]
        public double Longitude { get; set; }

        [Required(ErrorMessage = "Radius is required")]
        [Range(0.5, 50, ErrorMessage = "Radius must be between 0.5 and 50 km")]
        public double RadiusInKm { get; set; } = 5;
    }
}
