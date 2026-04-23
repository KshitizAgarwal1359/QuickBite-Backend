using System.Text.Json.Serialization;

namespace QuickBite.Order.Entities
{
    public class OrderItem
    {
        public int OrderItemId { get; set; }
        public int OrderId { get; set; }
        public int MenuItemId { get; set; }
        public string Name { get; set; } = string.Empty;
        public double Price { get; set; }
        public int Quantity { get; set; }
        public string? Customization { get; set; }

        // Navigation property
        [JsonIgnore]
        public Order? Order { get; set; }
    }
}
