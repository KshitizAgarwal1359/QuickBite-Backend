namespace QuickBite.Delivery.DTOs
{
    public class AgentResponseDto
    {
        public int AgentId { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string VehicleType { get; set; } = string.Empty;
        public string VehicleNumber { get; set; } = string.Empty;
        public double CurrentLatitude { get; set; }
        public double CurrentLongitude { get; set; }
        public bool IsAvailable { get; set; }
        public bool IsVerified { get; set; }
        public double AvgRating { get; set; }
        public int TotalDeliveries { get; set; }
    }
    
    public class AgentDistanceResponseDto : AgentResponseDto
    {
        public double DistanceInKm { get; set; }
    }
}
