using Microsoft.EntityFrameworkCore;
using QuickBite.Order.Data;
using QuickBite.Order.Interfaces;

namespace QuickBite.Order.Repository
{
    public class OrderRepository : IOrderRepository
    {
        private readonly OrderDbContext _context;

        public OrderRepository(OrderDbContext context)
        {
            _context = context;
        }

        public async Task<Entities.Order?> GetByIdAsync(int orderId)
        {
            return await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);
        }

        public async Task<List<Entities.Order>> GetByCustomerIdAsync(int customerId)
        {
            return await _context.Orders
                .Where(o => o.CustomerId == customerId)
                .Include(o => o.Items)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<List<Entities.Order>> GetByRestaurantIdAsync(int restaurantId)
        {
            return await _context.Orders
                .Where(o => o.RestaurantId == restaurantId)
                .Include(o => o.Items)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<List<Entities.Order>> GetByAgentIdAsync(int agentId)
        {
            return await _context.Orders
                .Where(o => o.DeliveryAgentId == agentId)
                .Include(o => o.Items)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<List<Entities.Order>> GetActiveOrdersAsync()
        {
            var activeStatuses = new[] { "PLACED", "CONFIRMED", "PREPARING", "PICKED_UP" };
            return await _context.Orders
                .Where(o => activeStatuses.Contains(o.OrderStatus))
                .Include(o => o.Items)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<int> CountByRestaurantIdAsync(int restaurantId)
        {
            return await _context.Orders.CountAsync(o => o.RestaurantId == restaurantId);
        }

        public async Task<Entities.Order> AddAsync(Entities.Order order)
        {
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task UpdateAsync(Entities.Order order)
        {
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
        }
    }
}
