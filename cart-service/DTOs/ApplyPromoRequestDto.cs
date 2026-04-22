using System.ComponentModel.DataAnnotations;

namespace QuickBite.Cart.DTOs
{
    public class ApplyPromoRequestDto
    {
        [Required]
        public int CartId { get; set; }

        [Required, StringLength(50)]
        public string PromoCode { get; set; } = string.Empty;
    }
}
