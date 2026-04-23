namespace QuickBite.Order.DTOs
{
    public class OrderResponseDto
    {
        public int OrderId { get; set; }
        public int CustomerId { get; set; }
        public int RestaurantId { get; set; }
        public int? DeliveryAgentId { get; set; }
        public double TotalAmount { get; set; }
        public double Discount { get; set; }
        public double FinalAmount { get; set; }
        public string ModeOfPayment { get; set; } = string.Empty;
        public string OrderStatus { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public DateTime? EstimatedDelivery { get; set; }
        public string DeliveryAddress { get; set; } = string.Empty;
        public string? SpecialInstructions { get; set; }

        public List<OrderItemResponseDto> Items { get; set; } = new();
    }
}
