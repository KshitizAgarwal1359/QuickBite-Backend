using System.Text.Json.Serialization;

namespace QuickBite.Cart.Entities
{
    public class CartItem
    {
        public int ItemId { get; set; }
        public int CartId { get; set; }
        public int MenuItemId { get; set; }
        public string Name { get; set; } = string.Empty;
        public double Price { get; set; }
        public int Quantity { get; set; } = 1;
        public string? Customization { get; set; }

        [JsonIgnore]
        public Cart? Cart { get; set; }
    }
}
