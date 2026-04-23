using System.Text.Json.Serialization;

namespace QuickBite.Payment.Entities
{
    public class WalletStatement
    {
        public int StatementId { get; set; }
        public int WalletId { get; set; }
        public double Amount { get; set; }
        
        /// <summary>
        /// DEPOSIT / DEBIT
        /// </summary>
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [JsonIgnore]
        public Wallet? Wallet { get; set; }
    }
}
