namespace QuickBite.Review.DTOs
{
    public class ReviewResponseDto
    {
        public int ReviewId { get; set; }
        public int OrderId { get; set; }
        public int CustomerId { get; set; }
        public int RestaurantId { get; set; }
        public int? AgentId { get; set; }
        public int FoodRating { get; set; }
        public int DeliveryRating { get; set; }
        public string? Comment { get; set; }
        public DateTime ReviewDate { get; set; }
        public bool IsVerifiedOnly { get; set; }
    }
}
