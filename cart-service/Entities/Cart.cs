using System.Text.Json.Serialization;

namespace QuickBite.Cart.Entities
{
    public class Cart
    {
        public int CartId { get; set; }
        public int CustomerId { get; set; }
        public int RestaurantId { get; set; }
        public double TotalPrice { get; set; } = 0;
        public double DiscountAmount { get; set; } = 0;
        public string? PromoCode { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [JsonIgnore]
        public List<CartItem> Items { get; set; } = new();
    }
}
