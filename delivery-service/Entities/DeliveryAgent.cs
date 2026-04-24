namespace QuickBite.Delivery.Entities
{
    public class DeliveryAgent
    {
        public int AgentId { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        
        /// <summary>
        /// BIKE / SCOOTER / CYCLE
        /// </summary>
        public string VehicleType { get; set; } = string.Empty;
        public string VehicleNumber { get; set; } = string.Empty;
        
        public double CurrentLatitude { get; set; }
        public double CurrentLongitude { get; set; }
        
        public bool IsAvailable { get; set; } = false;
        public bool IsVerified { get; set; } = false;
        
        public double AvgRating { get; set; } = 0.0;
        public int TotalDeliveries { get; set; } = 0;
    }
}
