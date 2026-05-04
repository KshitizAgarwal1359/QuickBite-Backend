namespace QuickBite.Review.Interfaces
{
    public interface IReviewRepository
    {
        Task<Entities.Review> AddAsync(Entities.Review review);
        Task<Entities.Review?> GetByIdAsync(int reviewId);
        Task<IEnumerable<Entities.Review>> FindByRestaurantIdAsync(int restaurantId);
        Task<IEnumerable<Entities.Review>> FindByCustomerIdAsync(int customerId);
        Task<Entities.Review?> FindByOrderIdAsync(int orderId);
        Task<IEnumerable<Entities.Review>> FindByAgentIdAsync(int agentId);
        Task<IEnumerable<Entities.Review>> GetAllAsync();
        Task UpdateAsync(Entities.Review review);
        Task DeleteAsync(Entities.Review review);
        
        Task<double> AvgFoodRatingByRestaurantIdAsync(int restaurantId);
        Task<double> AvgDeliveryRatingByAgentIdAsync(int agentId);
        Task<int> CountByRestaurantIdAsync(int restaurantId);
        Task<bool> ExistsByOrderIdAsync(int orderId);
    }
}
