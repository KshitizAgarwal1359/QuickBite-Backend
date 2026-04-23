using System.ComponentModel.DataAnnotations;

namespace QuickBite.Order.DTOs
{
    // Simulating the Cart item received during order placement.
    // In a full event-driven system, this might be fetched directly from Cart-Service.
    public class CartItemDto
    {
        public int MenuItemId { get; set; }
        public string Name { get; set; } = string.Empty;
        public double Price { get; set; }
        public int Quantity { get; set; }
        public string? Customization { get; set; }
    }

    public class PlaceOrderRequestDto
    {
        [Required]
        public int RestaurantId { get; set; }

        [Required, MinLength(10)]
        public string DeliveryAddress { get; set; } = string.Empty;

        [Required, RegularExpression("^(CARD|UPI|WALLET|COD)$", ErrorMessage = "Invalid ModeOfPayment")]
        public string ModeOfPayment { get; set; } = string.Empty;

        public string? SpecialInstructions { get; set; }

        public double Discount { get; set; } = 0;

        [Required, MinLength(1, ErrorMessage = "At least one item is required to place an order.")]
        public List<CartItemDto> Items { get; set; } = new();
    }
}
