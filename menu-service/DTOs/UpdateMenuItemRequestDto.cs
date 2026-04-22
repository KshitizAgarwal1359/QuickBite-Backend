using System.ComponentModel.DataAnnotations;

namespace QuickBite.Menu.DTOs
{
    public class UpdateMenuItemRequestDto
    {
        [StringLength(100, MinimumLength = 2)]
        public string? Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [Range(1, 50000)]
        public double? Price { get; set; }

        [Range(0, 50000)]
        public double? DiscountedPrice { get; set; }

        [StringLength(500)]
        public string? ImageUrl { get; set; }

        public bool? IsVeg { get; set; }

        [Range(0, 5000)]
        public int? Calories { get; set; }

        [StringLength(256)]
        public string? Tags { get; set; }
    }
}
