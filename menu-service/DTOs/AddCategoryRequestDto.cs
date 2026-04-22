using System.ComponentModel.DataAnnotations;

namespace QuickBite.Menu.DTOs
{
    public class AddCategoryRequestDto
    {
        [Required(ErrorMessage = "Restaurant ID is required")]
        public int RestaurantId { get; set; }

        [Required(ErrorMessage = "Category name is required")]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [StringLength(300)]
        public string Description { get; set; } = string.Empty;

        [StringLength(500)]
        public string? ImageUrl { get; set; }

        [Range(0, 100)]
        public int DisplayOrder { get; set; } = 0;
    }
}
