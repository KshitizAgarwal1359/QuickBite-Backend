using System.ComponentModel.DataAnnotations;

namespace QuickBite.Order.DTOs
{
    public class AssignAgentRequestDto
    {
        [Required]
        public int DeliveryAgentId { get; set; }
    }
}
