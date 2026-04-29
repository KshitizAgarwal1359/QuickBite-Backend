namespace QuickBite.Restaurant.DTOs
{
    public class RestaurantResponseDto
    {
        public int RestaurantId { get; set; }
        public int OwnerId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Cuisine { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? Phone { get; set; }
        public string? ImageUrl { get; set; }
        public double AvgRating { get; set; }
        public bool IsOpen { get; set; }
        public bool IsApproved { get; set; }
        public double DeliveryRadius { get; set; }
        public double MinOrderAmount { get; set; }
        public int EstimatedDeliveryMin { get; set; }
    }
}
