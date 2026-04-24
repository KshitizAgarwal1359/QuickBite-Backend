using System.ComponentModel.DataAnnotations;

namespace QuickBite.Delivery.DTOs
{
    public class AgentRegistrationRequestDto
    {
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [Phone]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [RegularExpression("^(BIKE|SCOOTER|CYCLE)$", ErrorMessage = "Vehicle must be BIKE, SCOOTER, or CYCLE")]
        public string VehicleType { get; set; } = string.Empty;

        [Required]
        public string VehicleNumber { get; set; } = string.Empty;
    }

    public class LocationUpdateRequestDto
    {
        [Required]
        [Range(-90, 90)]
        public double Latitude { get; set; }

        [Required]
        [Range(-180, 180)]
        public double Longitude { get; set; }
    }

    public class AssignOrderRequestDto
    {
        [Required]
        public int OrderId { get; set; }
    }

    public class AgentRatingRequestDto
    {
        [Required]
        [Range(1, 5)]
        public double Rating { get; set; }
    }
}
