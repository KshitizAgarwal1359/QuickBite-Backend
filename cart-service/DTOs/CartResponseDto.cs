namespace QuickBite.Cart.DTOs
{
    public class CartResponseDto
    {
        public int CartId { get; set; }
        public int CustomerId { get; set; }
        public int RestaurantId { get; set; }
        public double TotalPrice { get; set; }
        public double DiscountAmount { get; set; }
        public string? PromoCode { get; set; }
        public double FinalPrice { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<CartItemResponseDto> Items { get; set; } = new();
    }
}
