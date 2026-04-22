using System.ComponentModel.DataAnnotations;

namespace QuickBite.Cart.DTOs
{
    public class UpdateQuantityRequestDto
    {
        [Required]
        public int CartId { get; set; }

        [Required]
        public int ItemId { get; set; }

        [Required, Range(0, 50, ErrorMessage = "Quantity must be between 0 and 50. Set to 0 to remove.")]
        public int Quantity { get; set; }
    }
}
