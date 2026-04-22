namespace QuickBite.Menu.DTOs
{
    public class MenuItemResponseDto
    {
        public int ItemId { get; set; }
        public int RestaurantId { get; set; }
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Price { get; set; }
        public double DiscountedPrice { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsVeg { get; set; }
        public bool IsAvailable { get; set; }
        public double Rating { get; set; }
        public int Calories { get; set; }
        public string? Tags { get; set; }
    }
}
