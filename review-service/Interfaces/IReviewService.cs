using QuickBite.Review.DTOs;

namespace QuickBite.Review.Interfaces
{
    public interface IReviewService
    {
        Task<ReviewResponseDto> AddReviewAsync(int customerId, CreateReviewDto request);
        Task<IEnumerable<ReviewResponseDto>> GetByRestaurantAsync(int restaurantId);
        Task<IEnumerable<ReviewResponseDto>> GetByCustomerAsync(int customerId);
        Task<ReviewResponseDto> GetByOrderAsync(int orderId);
        Task<IEnumerable<ReviewResponseDto>> GetByAgentAsync(int agentId);
        Task<IEnumerable<ReviewResponseDto>> GetAllReviewsAsync();
        Task<ReviewResponseDto> UpdateReviewAsync(int reviewId, int customerId, UpdateReviewDto request);
        Task DeleteReviewAsync(int reviewId);
        Task<double> GetAvgFoodRatingAsync(int restaurantId);
        Task<double> GetAvgDeliveryRatingAsync(int agentId);
    }
}
