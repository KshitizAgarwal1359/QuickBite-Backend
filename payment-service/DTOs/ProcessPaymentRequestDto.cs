using System.ComponentModel.DataAnnotations;

namespace QuickBite.Payment.DTOs
{
    public class ProcessPaymentRequestDto
    {
        [Required]
        public int OrderId { get; set; }
        
        [Required]
        [Range(1, 100000, ErrorMessage = "Invalid amount")]
        public double Amount { get; set; }

        [Required]
        [RegularExpression("^(CARD|UPI|WALLET|COD)$", ErrorMessage = "Invalid Payment Mode")]
        public string Mode { get; set; } = string.Empty;

        // If paying via card/upi, the frontend should send the razorpay payment id.
        // If COD or Wallet, this is optional.
        public string? RazorpayPaymentId { get; set; }
        
        public string? RazorpayOrderId { get; set; }
        public string? RazorpaySignature { get; set; }
    }
}
