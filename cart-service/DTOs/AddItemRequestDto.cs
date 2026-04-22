using System.ComponentModel.DataAnnotations;

namespace QuickBite.Cart.DTOs
{
    public class AddItemRequestDto
    {
        [Required]
        public int RestaurantId { get; set; }

        [Required]
        public int MenuItemId { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, Range(0.01, 50000)]
        public double Price { get; set; }

        [Required, Range(1, 50)]
        public int Quantity { get; set; } = 1;

        [StringLength(256)]
        public string? Customization { get; set; }
    }
}
