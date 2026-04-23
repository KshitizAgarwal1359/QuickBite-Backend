using System.Text.Json.Serialization;

namespace QuickBite.Payment.Entities
{
    public class Wallet
    {
        public int WalletId { get; set; }
        public int CustomerId { get; set; }
        public double Balance { get; set; }

        [JsonIgnore]
        public List<WalletStatement> Statements { get; set; } = new();
    }
}
