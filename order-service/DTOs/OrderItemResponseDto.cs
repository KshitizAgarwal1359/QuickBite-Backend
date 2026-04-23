namespace QuickBite.Order.DTOs
{
    public class OrderItemResponseDto
    {
        public int OrderItemId { get; set; }
        public int MenuItemId { get; set; }
        public string Name { get; set; } = string.Empty;
        public double Price { get; set; }
        public int Quantity { get; set; }
        public string? Customization { get; set; }
        public double SubTotal { get; set; }
    }
}
