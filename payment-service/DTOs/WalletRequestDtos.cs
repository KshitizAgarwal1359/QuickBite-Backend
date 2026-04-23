using System.ComponentModel.DataAnnotations;

namespace QuickBite.Payment.DTOs
{
    public class WalletTopupRequestDto
    {
        [Required]
        [Range(1, 50000, ErrorMessage = "Top-up amount must be between ₹1 and ₹50,000")]
        public double Amount { get; set; }

        [Required]
        public string RazorpayPaymentId { get; set; } = string.Empty;
    }

    public class WalletPayRequestDto
    {
        [Required]
        public int OrderId { get; set; }
        
        [Required]
        public double Amount { get; set; }
    }
}
