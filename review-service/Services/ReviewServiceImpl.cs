using QuickBite.Review.DTOs;
using QuickBite.Review.Interfaces;

namespace QuickBite.Review.Services
{
    public class ReviewServiceImpl : IReviewService
    {
        private readonly IReviewRepository _reviewRepo;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ReviewServiceImpl> _logger;

        public ReviewServiceImpl(IReviewRepository reviewRepo, IHttpClientFactory httpClientFactory, ILogger<ReviewServiceImpl> logger)
        {
            _reviewRepo = reviewRepo;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<ReviewResponseDto> AddReviewAsync(int customerId, CreateReviewDto request)
        {
            if (await _reviewRepo.ExistsByOrderIdAsync(request.OrderId))
            {
                throw new InvalidOperationException("A review for this order has already been submitted.");
            }

            var review = new Entities.Review
            {
                OrderId = request.OrderId,
                CustomerId = customerId,
                RestaurantId = request.RestaurantId,
                AgentId = request.AgentId,
                FoodRating = request.FoodRating,
                DeliveryRating = request.DeliveryRating,
                Comment = request.Comment,
                ReviewDate = DateTime.UtcNow,
                IsVerifiedOnly = false
            };

            await _reviewRepo.AddAsync(review);
            _logger.LogInformation("Added dual review for OrderId: {OrderId}", review.OrderId);

            await PushAvgFoodRatingAsync(review.RestaurantId);
            if (review.AgentId.HasValue)
            {
                await PushAvgDeliveryRatingAsync(review.AgentId.Value);
            }

            return MapToResponse(review);
        }

        public async Task<ReviewResponseDto> UpdateReviewAsync(int reviewId, int customerId, UpdateReviewDto request)
        {
            var review = await _reviewRepo.GetByIdAsync(reviewId);
            if (review == null) throw new KeyNotFoundException($"Review {reviewId} not found");
            
            if (review.CustomerId != customerId)
                throw new UnauthorizedAccessException("You can only edit your own reviews.");

            review.FoodRating = request.FoodRating;
            review.DeliveryRating = request.DeliveryRating;
            review.Comment = request.Comment;
            
            await _reviewRepo.UpdateAsync(review);
            _logger.LogInformation("Customer {CustomerId} updated Review {ReviewId}", customerId, reviewId);

            await PushAvgFoodRatingAsync(review.RestaurantId);
            if (review.AgentId.HasValue)
            {
                await PushAvgDeliveryRatingAsync(review.AgentId.Value);
            }

            return MapToResponse(review);
        }

        public async Task DeleteReviewAsync(int reviewId)
        {
            var review = await _reviewRepo.GetByIdAsync(reviewId);
            if (review == null) throw new KeyNotFoundException($"Review {reviewId} not found");

            await _reviewRepo.DeleteAsync(review);
            _logger.LogInformation("Admin deleted Review {ReviewId}", reviewId);

            await PushAvgFoodRatingAsync(review.RestaurantId);
            if (review.AgentId.HasValue)
            {
                await PushAvgDeliveryRatingAsync(review.AgentId.Value);
            }
        }

        public async Task<IEnumerable<ReviewResponseDto>> GetByRestaurantAsync(int restaurantId)
        {
            var list = await _reviewRepo.FindByRestaurantIdAsync(restaurantId);
            return list.Select(MapToResponse);
        }

        public async Task<IEnumerable<ReviewResponseDto>> GetByCustomerAsync(int customerId)
        {
            var list = await _reviewRepo.FindByCustomerIdAsync(customerId);
            return list.Select(MapToResponse);
        }

        public async Task<ReviewResponseDto> GetByOrderAsync(int orderId)
        {
            var review = await _reviewRepo.FindByOrderIdAsync(orderId);
            if (review == null) throw new KeyNotFoundException($"Review for order {orderId} not found");
            return MapToResponse(review);
        }

        public async Task<IEnumerable<ReviewResponseDto>> GetByAgentAsync(int agentId)
        {
            var list = await _reviewRepo.FindByAgentIdAsync(agentId);
            return list.Select(MapToResponse);
        }

        public async Task<IEnumerable<ReviewResponseDto>> GetAllReviewsAsync()
        {
            var list = await _reviewRepo.GetAllAsync();
            return list.Select(MapToResponse);
        }

        public async Task<double> GetAvgFoodRatingAsync(int restaurantId)
        {
            return await _reviewRepo.AvgFoodRatingByRestaurantIdAsync(restaurantId);
        }

        public async Task<double> GetAvgDeliveryRatingAsync(int agentId)
        {
            return await _reviewRepo.AvgDeliveryRatingByAgentIdAsync(agentId);
        }

        private async Task PushAvgFoodRatingAsync(int restaurantId)
        {
            try
            {
                var avg = await _reviewRepo.AvgFoodRatingByRestaurantIdAsync(restaurantId);
                var client = _httpClientFactory.CreateClient("RestaurantService");
                var content = JsonContent.Create(new { Rating = avg });
                await client.PutAsync($"/api/v1/restaurants/{restaurantId}/rating", content);
                _logger.LogInformation("Pushed avg food rating {Avg} to Restaurant {RestId}", avg, restaurantId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to push avg food rating to RestaurantService");
            }
        }

        private async Task PushAvgDeliveryRatingAsync(int agentId)
        {
            try
            {
                var avg = await _reviewRepo.AvgDeliveryRatingByAgentIdAsync(agentId);
                var client = _httpClientFactory.CreateClient("DeliveryService");
                var content = JsonContent.Create(new { Rating = avg });
                await client.PutAsync($"/api/v1/deliveries/agent/{agentId}/rating", content);
                _logger.LogInformation("Pushed avg delivery rating {Avg} to Agent {AgentId}", avg, agentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to push avg delivery rating to DeliveryService");
            }
        }

        private ReviewResponseDto MapToResponse(Entities.Review review)
        {
            return new ReviewResponseDto
            {
                ReviewId = review.ReviewId,
                OrderId = review.OrderId,
                CustomerId = review.CustomerId,
                RestaurantId = review.RestaurantId,
                AgentId = review.AgentId,
                FoodRating = review.FoodRating,
                DeliveryRating = review.DeliveryRating,
                Comment = review.Comment,
                ReviewDate = review.ReviewDate,
                IsVerifiedOnly = review.IsVerifiedOnly
            };
        }
    }
}
