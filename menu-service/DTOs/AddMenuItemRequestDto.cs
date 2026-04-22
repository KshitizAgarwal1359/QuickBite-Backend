using System.ComponentModel.DataAnnotations;

namespace QuickBite.Menu.DTOs
{
    public class AddMenuItemRequestDto
    {
        [Required(ErrorMessage = "Restaurant ID is required")]
        public int RestaurantId { get; set; }

        [Required(ErrorMessage = "Category ID is required")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Item name is required")]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Price is required")]
        [Range(1, 50000, ErrorMessage = "Price must be between 1 and 50000")]
        public double Price { get; set; }

        [Range(0, 50000)]
        public double DiscountedPrice { get; set; }

        [StringLength(500)]
        public string? ImageUrl { get; set; }

        public bool IsVeg { get; set; }

        [Range(0, 5000, ErrorMessage = "Calories must be between 0 and 5000")]
        public int Calories { get; set; }

        [StringLength(256)]
        public string? Tags { get; set; }
    }
}
