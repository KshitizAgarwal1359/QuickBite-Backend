using QuickBite.Delivery.DTOs;

namespace QuickBite.Delivery.Interfaces
{
    public interface IDeliveryService
    {
        Task<AgentResponseDto> RegisterAgentAsync(int userId, AgentRegistrationRequestDto request);
        Task<AgentResponseDto> GetAgentByIdAsync(int agentId);
        Task<AgentResponseDto> GetAgentByUserIdAsync(int userId);
        Task<List<AgentResponseDto>> GetAllAgentsAsync();
        Task<List<AgentDistanceResponseDto>> GetNearbyAgentsAsync(double latitude, double longitude, double radiusInKm = 5);
        Task<AgentResponseDto> UpdateLocationAsync(int userId, LocationUpdateRequestDto request);
        Task<AgentResponseDto> SetAvailabilityAsync(int userId, bool isAvailable, bool forceOffline = false);
        Task<AgentResponseDto> VerifyAgentAsync(int agentId);
        Task<AgentResponseDto> AssignOrderAsync(int agentId, int orderId);
        Task<AgentResponseDto> CompleteDeliveryAsync(int userId, int orderId);
        Task<AgentResponseDto> UpdateRatingAsync(int agentId, double rating);
        Task<List<int>> GetActiveDeliveriesAsync(int userId); // Returns orderIds assigned to this agent currently
    }
}
