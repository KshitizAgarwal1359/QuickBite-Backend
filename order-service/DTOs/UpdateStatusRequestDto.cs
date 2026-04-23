using System.ComponentModel.DataAnnotations;

namespace QuickBite.Order.DTOs
{
    public class UpdateStatusRequestDto
    {
        [Required]
        [RegularExpression("^(CONFIRMED|PREPARING|PICKED_UP|DELIVERED|CANCELLED)$", ErrorMessage = "Invalid OrderStatus")]
        public string OrderStatus { get; set; } = string.Empty;
    }
}
