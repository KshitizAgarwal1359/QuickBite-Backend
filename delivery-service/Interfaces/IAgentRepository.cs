using QuickBite.Delivery.Entities;

namespace QuickBite.Delivery.Interfaces
{
    public interface IAgentRepository
    {
        Task<DeliveryAgent?> GetByUserIdAsync(int userId);
        Task<DeliveryAgent?> GetByAgentIdAsync(int agentId);
        Task<List<DeliveryAgent>> GetAvailableAndVerifiedAsync();
        Task<DeliveryAgent> AddAsync(DeliveryAgent agent);
        Task UpdateAsync(DeliveryAgent agent);
    }
}
