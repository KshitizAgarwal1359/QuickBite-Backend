using Microsoft.EntityFrameworkCore;
using QuickBite.Review.Data;
using QuickBite.Review.Interfaces;

namespace QuickBite.Review.Repository
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly ReviewDbContext _context;

        public ReviewRepository(ReviewDbContext context)
        {
            _context = context;
        }

        public async Task<Entities.Review> AddAsync(Entities.Review review)
        {
            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();
            return review;
        }

        public async Task<Entities.Review?> GetByIdAsync(int reviewId)
        {
            return await _context.Reviews.FindAsync(reviewId);
        }

        public async Task<IEnumerable<Entities.Review>> FindByRestaurantIdAsync(int restaurantId)
        {
            return await _context.Reviews.Where(r => r.RestaurantId == restaurantId).ToListAsync();
        }

        public async Task<IEnumerable<Entities.Review>> FindByCustomerIdAsync(int customerId)
        {
            return await _context.Reviews.Where(r => r.CustomerId == customerId).ToListAsync();
        }

        public async Task<Entities.Review?> FindByOrderIdAsync(int orderId)
        {
            return await _context.Reviews.FirstOrDefaultAsync(r => r.OrderId == orderId);
        }

        public async Task<IEnumerable<Entities.Review>> FindByAgentIdAsync(int agentId)
        {
            return await _context.Reviews.Where(r => r.AgentId == agentId).ToListAsync();
        }

        public async Task<IEnumerable<Entities.Review>> GetAllAsync()
        {
            return await _context.Reviews.ToListAsync();
        }

        public async Task UpdateAsync(Entities.Review review)
        {
            _context.Reviews.Update(review);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Entities.Review review)
        {
            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();
        }

        public async Task<double> AvgFoodRatingByRestaurantIdAsync(int restaurantId)
        {
            var count = await _context.Reviews.CountAsync(r => r.RestaurantId == restaurantId);
            if (count == 0) return 0;
            return await _context.Reviews.Where(r => r.RestaurantId == restaurantId).AverageAsync(r => r.FoodRating);
        }

        public async Task<double> AvgDeliveryRatingByAgentIdAsync(int agentId)
        {
            var count = await _context.Reviews.CountAsync(r => r.AgentId == agentId);
            if (count == 0) return 0;
            return await _context.Reviews.Where(r => r.AgentId == agentId).AverageAsync(r => r.DeliveryRating);
        }

        public async Task<int> CountByRestaurantIdAsync(int restaurantId)
        {
            return await _context.Reviews.CountAsync(r => r.RestaurantId == restaurantId);
        }

        public async Task<bool> ExistsByOrderIdAsync(int orderId)
        {
            return await _context.Reviews.AnyAsync(r => r.OrderId == orderId);
        }
    }
}
