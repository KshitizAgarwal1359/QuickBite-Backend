using System.Text.Json.Serialization;

namespace QuickBite.Payment.Entities
{
    public class Payment
    {
        public int PaymentId { get; set; }
        public int OrderId { get; set; }
        public int CustomerId { get; set; }
        public double Amount { get; set; }
        
        /// <summary>
        /// PENDING / PAID / REFUNDED / FAILED
        /// </summary>
        public string Status { get; set; } = "PENDING";
        
        /// <summary>
        /// CARD / UPI / WALLET / COD
        /// </summary>
        public string Mode { get; set; } = string.Empty;
        
        public string? TransactionId { get; set; }
        public string Currency { get; set; } = "INR";
        
        public DateTime? PaidAt { get; set; }
        public DateTime? RefundedAt { get; set; }
    }
}
