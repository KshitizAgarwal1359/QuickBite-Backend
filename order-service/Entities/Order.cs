using System.Text.Json.Serialization;

namespace QuickBite.Order.Entities
{
    public class Order
    {
        public int OrderId { get; set; }
        public int CustomerId { get; set; }
        public int RestaurantId { get; set; }
        public int? DeliveryAgentId { get; set; }
        public double TotalAmount { get; set; }
        public double Discount { get; set; }
        public double FinalAmount { get; set; }
        public string ModeOfPayment { get; set; } = string.Empty;
        public string OrderStatus { get; set; } = "PLACED";
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public DateTime? EstimatedDelivery { get; set; }
        public string DeliveryAddress { get; set; } = string.Empty;
        public string? SpecialInstructions { get; set; }

        // Navigation property
        [JsonIgnore]
        public List<OrderItem> Items { get; set; } = new();
    }
}
