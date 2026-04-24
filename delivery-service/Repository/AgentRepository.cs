using Microsoft.EntityFrameworkCore;
using QuickBite.Delivery.Data;
using QuickBite.Delivery.Entities;
using QuickBite.Delivery.Interfaces;

namespace QuickBite.Delivery.Repository
{
    public class AgentRepository : IAgentRepository
    {
        private readonly DeliveryDbContext _context;

        public AgentRepository(DeliveryDbContext context) { _context = context; }

        public async Task<DeliveryAgent?> GetByUserIdAsync(int userId)
        {
            return await _context.DeliveryAgents.FirstOrDefaultAsync(a => a.UserId == userId);
        }

        public async Task<DeliveryAgent?> GetByAgentIdAsync(int agentId)
        {
            return await _context.DeliveryAgents.FirstOrDefaultAsync(a => a.AgentId == agentId);
        }

        public async Task<List<DeliveryAgent>> GetAvailableAndVerifiedAsync()
        {
            return await _context.DeliveryAgents
                .Where(a => a.IsAvailable && a.IsVerified)
                .ToListAsync();
        }

        public async Task<DeliveryAgent> AddAsync(DeliveryAgent agent)
        {
            await _context.DeliveryAgents.AddAsync(agent);
            await _context.SaveChangesAsync();
            return agent;
        }

        public async Task UpdateAsync(DeliveryAgent agent)
        {
            _context.DeliveryAgents.Update(agent);
            await _context.SaveChangesAsync();
        }
    }
}
