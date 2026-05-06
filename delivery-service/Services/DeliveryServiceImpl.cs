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
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;
        private readonly ILogger<DeliveryServiceImpl> _logger;

        public DeliveryServiceImpl(
            IAgentRepository agentRepo,
            IHubContext<LocationHub> hubContext,
            IHttpClientFactory httpClientFactory,
            IConfiguration config,
            ILogger<DeliveryServiceImpl> logger)
        {
            _agentRepo = agentRepo;
            _hubContext = hubContext;
            _httpClientFactory = httpClientFactory;
            _config = config;
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

        public async Task<List<AgentResponseDto>> GetAllAgentsAsync()
        {
            var agents = await _agentRepo.GetAllAsync();

            // Cross-check with auth-service: find any agents whose user account is now deactivated
            // and auto-correct their IsAvailable flag in the DB.
            // This fixes stale data (e.g. Chaman deactivated before the forceOffline webhook existed).
            var onlineAgents = agents.Where(a => a.IsAvailable).ToList();
            if (onlineAgents.Count > 0)
            {
                var inactiveIds = await GetInactiveUserIdsFromAuthAsync(
                    onlineAgents.Select(a => a.UserId).ToList());

                if (inactiveIds.Count > 0)
                {
                    foreach (var agent in onlineAgents.Where(a => inactiveIds.Contains(a.UserId)))
                    {
                        agent.IsAvailable = false;
                        await _agentRepo.UpdateAsync(agent);
                        _logger.LogWarning(
                            "Auto-corrected: Agent {AgentId} (UserId {UserId}) forced offline — account is deactivated",
                            agent.AgentId, agent.UserId);
                    }
                }
            }

            // Re-fetch after corrections so the response reflects the updated state
            var updated = await _agentRepo.GetAllAsync();
            return updated.Select(MapToResponse).ToList();
        }

        /// <summary>
        /// Calls auth-service to get the list of userIds (from the given set) whose accounts are inactive.
        /// Returns empty list if auth-service is unreachable — fail-open to avoid blocking admin page.
        /// </summary>
        private async Task<List<int>> GetInactiveUserIdsFromAuthAsync(List<int> userIds)
        {
            try
            {
                var authBaseUrl = _config["ServiceUrls:Auth"] ?? "http://localhost:5093";
                var secret = _config["InternalSecrets:ServiceKey"];

                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Add("X-Internal-Secret", secret);

                var response = await client.PostAsJsonAsync(
                    $"{authBaseUrl}/api/v1/auth/internal/inactive-user-ids", userIds);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Auth batch-check returned {Status}", response.StatusCode);
                    return [];
                }

                return await response.Content.ReadFromJsonAsync<List<int>>() ?? [];
            }
            catch (Exception ex)
            {
                // Fail-open: if auth-service is down, don’t block the admin page
                _logger.LogError(ex, "Failed to reach auth-service for inactive user check");
                return [];
            }
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

        public async Task<AgentResponseDto> SetAvailabilityAsync(int userId, bool isAvailable, bool forceOffline = false)
        {
            var agent = await _agentRepo.GetByUserIdAsync(userId);
            if (agent == null) throw new KeyNotFoundException("Agent not found");

            // Only enforce verified check when agent is trying to go ONLINE.
            // forceOffline=true bypasses this (used by account deactivation).
            if (isAvailable && !agent.IsVerified && !forceOffline)
                throw new InvalidOperationException("Unverified agents cannot go online.");

            agent.IsAvailable = isAvailable;
            await _agentRepo.UpdateAsync(agent);
            
            _logger.LogInformation("Agent {AgentId} availability changed to {IsAvailable}{Forced}", 
                agent.AgentId, isAvailable, forceOffline ? " (forced by system)" : "");

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
