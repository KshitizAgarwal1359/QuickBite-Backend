using Microsoft.AspNetCore.SignalR;
using QuickBite.Delivery.DTOs;
using QuickBite.Delivery.Entities;
using QuickBite.Delivery.Hubs;
using QuickBite.Delivery.Interfaces;

namespace QuickBite.Delivery.Services
{
    public class DeliveryServiceImpl : IDeliveryService
    {
        private readonly IAgentRepository _agentRepo;
        private readonly IHubContext<LocationHub> _hubContext;
        private readonly ILogger<DeliveryServiceImpl> _logger;

        // In a real system, you'd store active deliveries in Redis or a DB table (e.g. AgentDeliveries).
        // Since the requirements didn't specify a new table for active deliveries in this DB, 
        // we'll rely on the order-service calling complete/assign, or we can use a small in-memory dict for demo
        // (but ideally it would be queried from order-service).
        // For now, we'll just mock it or leave it empty, as the actual order state is in Order DB.
        
        public DeliveryServiceImpl(IAgentRepository agentRepo, IHubContext<LocationHub> hubContext, ILogger<DeliveryServiceImpl> logger)
        {
            _agentRepo = agentRepo;
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task<AgentResponseDto> RegisterAgentAsync(int userId, AgentRegistrationRequestDto request)
        {
            var existing = await _agentRepo.GetByUserIdAsync(userId);
            if (existing != null)
                throw new InvalidOperationException("User is already registered as an agent.");

            var agent = new DeliveryAgent
            {
                UserId = userId,
                FullName = request.FullName,
                Phone = request.Phone,
                VehicleType = request.VehicleType,
                VehicleNumber = request.VehicleNumber,
                IsAvailable = false,
                IsVerified = false
            };

            await _agentRepo.AddAsync(agent);
            _logger.LogInformation("Agent registered: {UserId}", userId);
            
            return MapToResponse(agent);
        }

        public async Task<AgentResponseDto> GetAgentByIdAsync(int agentId)
        {
            var agent = await _agentRepo.GetByAgentIdAsync(agentId);
            if (agent == null) throw new KeyNotFoundException("Agent not found");
            return MapToResponse(agent);
        }

        public async Task<AgentResponseDto> GetAgentByUserIdAsync(int userId)
        {
            var agent = await _agentRepo.GetByUserIdAsync(userId);
            if (agent == null) throw new KeyNotFoundException("Agent profile not found for this user");
            return MapToResponse(agent);
        }

        public async Task<List<AgentDistanceResponseDto>> GetNearbyAgentsAsync(double latitude, double longitude, double radiusInKm = 5)
        {
            var onlineAgents = await _agentRepo.GetAvailableAndVerifiedAsync();

            var nearbyAgents = new List<AgentDistanceResponseDto>();

            foreach (var agent in onlineAgents)
            {
                var distance = CalculateDistance(latitude, longitude, agent.CurrentLatitude, agent.CurrentLongitude);
                if (distance <= radiusInKm)
                {
                    var dto = new AgentDistanceResponseDto
                    {
                        AgentId = agent.AgentId,
                        UserId = agent.UserId,
                        FullName = agent.FullName,
                        Phone = agent.Phone,
                        VehicleType = agent.VehicleType,
                        VehicleNumber = agent.VehicleNumber,
                        CurrentLatitude = agent.CurrentLatitude,
                        CurrentLongitude = agent.CurrentLongitude,
                        IsAvailable = agent.IsAvailable,
                        IsVerified = agent.IsVerified,
                        AvgRating = agent.AvgRating,
                        TotalDeliveries = agent.TotalDeliveries,
                        DistanceInKm = Math.Round(distance, 2)
                    };
                    nearbyAgents.Add(dto);
                }
            }

            return nearbyAgents.OrderBy(a => a.DistanceInKm).ToList();
        }

        public async Task<AgentResponseDto> UpdateLocationAsync(int userId, LocationUpdateRequestDto request)
        {
            var agent = await _agentRepo.GetByUserIdAsync(userId);
            if (agent == null) throw new KeyNotFoundException("Agent not found");

            agent.CurrentLatitude = request.Latitude;
            agent.CurrentLongitude = request.Longitude;

            await _agentRepo.UpdateAsync(agent);

            // Broadcast via SignalR to anyone tracking this agent
            await _hubContext.Clients.Group($"Agent_{agent.AgentId}")
                .SendAsync("ReceiveLocationUpdate", new { AgentId = agent.AgentId, Latitude = request.Latitude, Longitude = request.Longitude });

            _logger.LogInformation("Agent {AgentId} location updated and broadcasted.", agent.AgentId);

            return MapToResponse(agent);
        }

        public async Task<AgentResponseDto> SetAvailabilityAsync(int userId, bool isAvailable)
        {
            var agent = await _agentRepo.GetByUserIdAsync(userId);
            if (agent == null) throw new KeyNotFoundException("Agent not found");

            if (isAvailable && !agent.IsVerified)
                throw new InvalidOperationException("Unverified agents cannot go online.");

            agent.IsAvailable = isAvailable;
            await _agentRepo.UpdateAsync(agent);
            
            _logger.LogInformation("Agent {AgentId} availability changed to {IsAvailable}", agent.AgentId, isAvailable);

            return MapToResponse(agent);
        }

        public async Task<AgentResponseDto> VerifyAgentAsync(int agentId)
        {
            var agent = await _agentRepo.GetByAgentIdAsync(agentId);
            if (agent == null) throw new KeyNotFoundException("Agent not found");

            agent.IsVerified = true;
            await _agentRepo.UpdateAsync(agent);

            _logger.LogInformation("Agent {AgentId} verified by ADMIN.", agent.AgentId);

            return MapToResponse(agent);
        }

        public async Task<AgentResponseDto> AssignOrderAsync(int agentId, int orderId)
        {
            var agent = await _agentRepo.GetByAgentIdAsync(agentId);
            if (agent == null) throw new KeyNotFoundException("Agent not found");

            if (!agent.IsAvailable || !agent.IsVerified)
                throw new InvalidOperationException("Agent is not available or not verified for assignment.");

            // In production, register the order id to the agent's active list
            // Or make an HTTP call to Order-Service to assign.
            _logger.LogInformation("Order {OrderId} assigned to Agent {AgentId}", orderId, agent.AgentId);

            return MapToResponse(agent);
        }

        public async Task<AgentResponseDto> CompleteDeliveryAsync(int userId, int orderId)
        {
            var agent = await _agentRepo.GetByUserIdAsync(userId);
            if (agent == null) throw new KeyNotFoundException("Agent not found");

            agent.TotalDeliveries += 1;
            await _agentRepo.UpdateAsync(agent);

            _logger.LogInformation("Agent {AgentId} completed delivery for Order {OrderId}. Total: {Total}", agent.AgentId, orderId, agent.TotalDeliveries);

            return MapToResponse(agent);
        }

        public async Task<AgentResponseDto> UpdateRatingAsync(int agentId, double rating)
        {
            var agent = await _agentRepo.GetByAgentIdAsync(agentId);
            if (agent == null) throw new KeyNotFoundException("Agent not found");

            // Simple moving average for demonstration
            if (agent.AvgRating == 0)
            {
                agent.AvgRating = rating;
            }
            else
            {
                // weighted average 
                int previousDeliveries = agent.TotalDeliveries == 0 ? 1 : agent.TotalDeliveries;
                agent.AvgRating = ((agent.AvgRating * previousDeliveries) + rating) / (previousDeliveries + 1);
                agent.AvgRating = Math.Round(agent.AvgRating, 1);
            }

            await _agentRepo.UpdateAsync(agent);
            _logger.LogInformation("Agent {AgentId} rating updated to {Rating}", agentId, agent.AvgRating);

            return MapToResponse(agent);
        }

        public async Task<List<int>> GetActiveDeliveriesAsync(int userId)
        {
            // Placeholder: Returns mock active order for the agent if needed.
            // Ideally fetches from the Order Service via HTTP.
            return await Task.FromResult(new List<int>()); 
        }

        private static AgentResponseDto MapToResponse(DeliveryAgent agent)
        {
            return new AgentResponseDto
            {
                AgentId = agent.AgentId,
                UserId = agent.UserId,
                FullName = agent.FullName,
                Phone = agent.Phone,
                VehicleType = agent.VehicleType,
                VehicleNumber = agent.VehicleNumber,
                CurrentLatitude = agent.CurrentLatitude,
                CurrentLongitude = agent.CurrentLongitude,
                IsAvailable = agent.IsAvailable,
                IsVerified = agent.IsVerified,
                AvgRating = agent.AvgRating,
                TotalDeliveries = agent.TotalDeliveries
            };
        }

        // Haversine formula
        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            var dLat = (lat2 - lat1) * Math.PI / 180.0;
            var dLon = (lon2 - lon1) * Math.PI / 180.0;
            var lat1Rad = lat1 * Math.PI / 180.0;
            var lat2Rad = lat2 * Math.PI / 180.0;

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2) * Math.Cos(lat1Rad) * Math.Cos(lat2Rad);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            // Earth radius in kilometers = 6371
            return 6371 * c;
        }
    }
}
