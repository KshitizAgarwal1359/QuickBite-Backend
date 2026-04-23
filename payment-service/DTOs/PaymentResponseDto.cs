namespace QuickBite.Payment.DTOs
{
    public class PaymentResponseDto
    {
        public int PaymentId { get; set; }
        public int OrderId { get; set; }
        public int CustomerId { get; set; }
        public double Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Mode { get; set; } = string.Empty;
        public string? TransactionId { get; set; }
        public string Currency { get; set; } = string.Empty;
        public DateTime? PaidAt { get; set; }
        public DateTime? RefundedAt { get; set; }
    }
}
