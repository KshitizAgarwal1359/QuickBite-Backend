namespace QuickBite.Menu.DTOs
{
    public class CategoryResponseDto
    {
        public int CategoryId { get; set; }
        public int RestaurantId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public int DisplayOrder { get; set; }
        public List<MenuItemResponseDto> Items { get; set; } = new();
    }
}
