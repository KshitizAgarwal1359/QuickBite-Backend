using System.ComponentModel.DataAnnotations;

namespace QuickBite.Menu.DTOs
{
    public class UpdateCategoryRequestDto
    {
        [StringLength(100, MinimumLength = 2)]
        public string? Name { get; set; }

        [StringLength(300)]
        public string? Description { get; set; }

        [StringLength(500)]
        public string? ImageUrl { get; set; }

        [Range(0, 100)]
        public int? DisplayOrder { get; set; }
    }
}
