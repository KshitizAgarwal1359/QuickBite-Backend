namespace QuickBite.Order.Interfaces
{
    public interface IOrderRepository
    {
        Task<Entities.Order?> GetByIdAsync(int orderId);
        Task<List<Entities.Order>> GetByCustomerIdAsync(int customerId);
        Task<List<Entities.Order>> GetByRestaurantIdAsync(int restaurantId);
        Task<List<Entities.Order>> GetByAgentIdAsync(int agentId);
        Task<List<Entities.Order>> GetActiveOrdersAsync();
        Task<int> CountByRestaurantIdAsync(int restaurantId);
        Task<Entities.Order> AddAsync(Entities.Order order);
        Task UpdateAsync(Entities.Order order);
    }
}
