namespace QuickBite.Cart.Entities
{
    public class PromoCode
    {
        public int PromoCodeId { get; set; }
        public string Code { get; set; } = string.Empty;
        public double DiscountPercent { get; set; }
        public double MaxDiscountAmount { get; set; }
        public double MinOrderValue { get; set; }
        public DateTime ExpiryDate { get; set; }
        public int UsageLimit { get; set; }
        public int TimesUsed { get; set; } = 0;
        public bool IsActive { get; set; } = true;
    }
}
