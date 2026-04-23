using QuickBite.Order.DTOs;

namespace QuickBite.Order.Interfaces
{
    public interface IOrderService
    {
        Task<OrderResponseDto> PlaceOrderAsync(int customerId, PlaceOrderRequestDto request);
        Task<OrderResponseDto> GetOrderByIdAsync(int orderId, int? userId = null, string? role = null);
        Task<List<OrderResponseDto>> GetOrdersByCustomerAsync(int customerId);
        Task<List<OrderResponseDto>> GetOrdersByRestaurantAsync(int restaurantId);
        Task<List<OrderResponseDto>> GetActiveOrdersAsync();
        Task<OrderResponseDto> UpdateOrderStatusAsync(int orderId, string newStatus);
        Task<OrderResponseDto> AssignDeliveryAgentAsync(int orderId, int agentId);
        Task<OrderResponseDto> CancelOrderAsync(int orderId, int customerId);
        Task<PlaceOrderRequestDto> ReorderFromHistoryAsync(int orderId, int customerId);
        Task<int> GetOrderCountForRestaurantAsync(int restaurantId);
    }
}
