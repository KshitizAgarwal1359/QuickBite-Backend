namespace QuickBite.Payment.DTOs
{
    public class WalletResponseDto
    {
        public int WalletId { get; set; }
        public int CustomerId { get; set; }
        public double Balance { get; set; }
    }

    public class WalletStatementResponseDto
    {
        public int StatementId { get; set; }
        public double Amount { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
