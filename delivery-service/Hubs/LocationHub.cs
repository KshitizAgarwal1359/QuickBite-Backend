using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace QuickBite.Delivery.Hubs
{
    public class LocationHub : Hub
    {
        private readonly ILogger<LocationHub> _logger;

        public LocationHub(ILogger<LocationHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation("Client Connected to LocationHub: {ConnectionId}", Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation("Client Disconnected from LocationHub: {ConnectionId}", Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

        // Clients can call this if they want to subscribe to a specific agent's updates
        public async Task SubscribeToAgent(string agentId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Agent_{agentId}");
            _logger.LogInformation("Client {ConnectionId} subscribed to Agent {AgentId}", Context.ConnectionId, agentId);
        }

        public async Task UnsubscribeFromAgent(string agentId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Agent_{agentId}");
        }
    }
}
