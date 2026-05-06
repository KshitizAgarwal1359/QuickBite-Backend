using QuickBite.Order.DTOs;
using QuickBite.Order.Entities;
using QuickBite.Order.Interfaces;

namespace QuickBite.Order.Services
{
    public class OrderServiceImpl : IOrderService
    {
        private readonly IOrderRepository _orderRepo;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;
        private readonly ILogger<OrderServiceImpl> _logger;

        public OrderServiceImpl(IOrderRepository orderRepo, IHttpClientFactory httpClientFactory, IConfiguration config, ILogger<OrderServiceImpl> logger)
        {
            _orderRepo = orderRepo;
            _httpClientFactory = httpClientFactory;
            _config = config;
            _logger = logger;
        }

        public async Task<OrderResponseDto> PlaceOrderAsync(int customerId, PlaceOrderRequestDto request)
        {
            double calculatedTotal = request.Items.Sum(i => i.Price * i.Quantity);
            double finalAmount = calculatedTotal - request.Discount;
            if (finalAmount < 0) finalAmount = 0;

            var order = new Entities.Order
            {
                CustomerId = customerId,
                RestaurantId = request.RestaurantId,
                TotalAmount = calculatedTotal,
                Discount = request.Discount,
                FinalAmount = finalAmount,
                ModeOfPayment = request.ModeOfPayment,
                OrderStatus = "PLACED",
                OrderDate = DateTime.UtcNow,
                EstimatedDelivery = DateTime.UtcNow.AddMinutes(45), // basic estimation
                DeliveryAddress = request.DeliveryAddress,
                SpecialInstructions = request.SpecialInstructions,
                Items = request.Items.Select(i => new OrderItem
                {
                    MenuItemId = i.MenuItemId,
                    Name = i.Name,
                    Price = i.Price,
                    Quantity = i.Quantity,
                    Customization = i.Customization
                }).ToList()
            };

            await _orderRepo.AddAsync(order);
            _logger.LogInformation("Order {OrderId} PLACED by Customer {CustomerId} for Restaurant {RestaurantId}", 
                order.OrderId, customerId, order.RestaurantId);

            return MapToResponse(order);
        }

        public async Task<OrderResponseDto> GetOrderByIdAsync(int orderId, int? userId = null, string? role = null)
        {
            var order = await _orderRepo.GetByIdAsync(orderId);
            if (order == null) throw new KeyNotFoundException($"Order {orderId} not found");

            // Basic authorization check
            if (role == "CUSTOMER" && order.CustomerId != userId)
                throw new UnauthorizedAccessException("You can only view your own orders");

            return MapToResponse(order);
        }

        public async Task<List<OrderResponseDto>> GetOrdersByCustomerAsync(int customerId)
        {
            var orders = await _orderRepo.GetByCustomerIdAsync(customerId);
            return orders.Select(MapToResponse).ToList();
        }

        public async Task<List<OrderResponseDto>> GetOrdersByRestaurantAsync(int restaurantId)
        {
            var orders = await _orderRepo.GetByRestaurantIdAsync(restaurantId);
            return orders.Select(MapToResponse).ToList();
        }

        public async Task<List<OrderResponseDto>> GetActiveOrdersAsync()
        {
            var orders = await _orderRepo.GetActiveOrdersAsync();
            return orders.Select(MapToResponse).ToList();
        }

        public async Task<OrderResponseDto> UpdateOrderStatusAsync(int orderId, string newStatus)
        {
            var order = await _orderRepo.GetByIdAsync(orderId);
            if (order == null) throw new KeyNotFoundException($"Order {orderId} not found");

            var validTransitions = new Dictionary<string, string[]>
            {
                { "PLACED", new[] { "CONFIRMED", "CANCELLED" } },
                { "CONFIRMED", new[] { "PREPARING", "CANCELLED" } },
                { "PREPARING", new[] { "PICKED_UP" } },
                { "PICKED_UP", new[] { "CUSTOMER_RECEIVED" } },
                { "CUSTOMER_RECEIVED", new[] { "DELIVERED" } },
                { "DELIVERED", Array.Empty<string>() },
                { "CANCELLED", Array.Empty<string>() }
            };

            if (!validTransitions.ContainsKey(order.OrderStatus) || !validTransitions[order.OrderStatus].Contains(newStatus))
            {
                throw new InvalidOperationException($"Cannot transition order from {order.OrderStatus} to {newStatus}");
            }

            order.OrderStatus = newStatus;
            await _orderRepo.UpdateAsync(order);

            _logger.LogInformation("Order {OrderId} status updated to {NewStatus}", orderId, newStatus);
            return MapToResponse(order);
        }

        public async Task<OrderResponseDto> AssignDeliveryAgentAsync(int orderId, int agentId)
        {
            var order = await _orderRepo.GetByIdAsync(orderId);
            if (order == null) throw new KeyNotFoundException($"Order {orderId} not found");

            order.DeliveryAgentId = agentId;
            await _orderRepo.UpdateAsync(order);

            _logger.LogInformation("Agent {AgentId} assigned to Order {OrderId}", agentId, orderId);
            return MapToResponse(order);
        }

        public async Task<OrderResponseDto> CustomerConfirmReceiptAsync(int orderId, int customerId)
        {
            var order = await _orderRepo.GetByIdAsync(orderId);
            if (order == null) throw new KeyNotFoundException($"Order {orderId} not found");

            if (order.CustomerId != customerId)
                throw new UnauthorizedAccessException("You can only confirm your own orders");

            if (order.OrderStatus != "PICKED_UP")
                throw new InvalidOperationException("Order must be picked up before confirming receipt.");

            order.OrderStatus = "CUSTOMER_RECEIVED";
            await _orderRepo.UpdateAsync(order);

            _logger.LogInformation("Order {OrderId} confirmed received by Customer {CustomerId}", orderId, customerId);
            return MapToResponse(order);
        }

        public async Task<OrderResponseDto> CancelOrderAsync(int orderId, int customerId)
        {
            var order = await _orderRepo.GetByIdAsync(orderId);
            if (order == null) throw new KeyNotFoundException($"Order {orderId} not found");

            if (order.CustomerId != customerId)
                throw new UnauthorizedAccessException("You can only cancel your own orders");

            if (order.OrderStatus != "PLACED" && order.OrderStatus != "CONFIRMED")
                throw new InvalidOperationException("Order cannot be cancelled once preparation has started");

            order.OrderStatus = "CANCELLED";
            await _orderRepo.UpdateAsync(order);

            if (order.ModeOfPayment != "COD")
            {
                _logger.LogInformation("Refund triggered for Order {OrderId} (Amount: {Amount})", orderId, order.FinalAmount);
                await TriggerRefundAsync(orderId);
            }

            _logger.LogWarning("Order {OrderId} CANCELLED by Customer {CustomerId}", orderId, customerId);
            return MapToResponse(order);
        }

        public async Task<OrderResponseDto> CancelOrderByOwnerAsync(int orderId, int restaurantId)
        {
            var order = await _orderRepo.GetByIdAsync(orderId);
            if (order == null) throw new KeyNotFoundException($"Order {orderId} not found");

            if (order.RestaurantId != restaurantId)
                throw new UnauthorizedAccessException("You can only cancel orders belonging to your restaurant");

            if (order.OrderStatus != "PLACED" && order.OrderStatus != "CONFIRMED")
                throw new InvalidOperationException("Order cannot be cancelled once preparation has started");

            order.OrderStatus = "CANCELLED";
            await _orderRepo.UpdateAsync(order);

            if (order.ModeOfPayment != "COD")
            {
                _logger.LogInformation("Refund triggered for Order {OrderId} (Amount: {Amount}) cancelled by Owner", orderId, order.FinalAmount);
                await TriggerRefundAsync(orderId);
            }

            _logger.LogWarning("Order {OrderId} CANCELLED by Owner (RestaurantId: {RestaurantId})", orderId, restaurantId);
            return MapToResponse(order);
        }

        public async Task<PlaceOrderRequestDto> ReorderFromHistoryAsync(int orderId, int customerId)
        {
            var order = await _orderRepo.GetByIdAsync(orderId);
            if (order == null) throw new KeyNotFoundException($"Order {orderId} not found");

            if (order.CustomerId != customerId)
                throw new UnauthorizedAccessException("You can only reorder your own past orders");

            return new PlaceOrderRequestDto
            {
                RestaurantId = order.RestaurantId,
                DeliveryAddress = order.DeliveryAddress,
                ModeOfPayment = order.ModeOfPayment, // Usually re-selected by user on frontend, but we prefill
                Items = order.Items.Select(i => new CartItemDto
                {
                    MenuItemId = i.MenuItemId,
                    Name = i.Name,
                    Price = i.Price, // In real world, might fetch updated prices from menu service
                    Quantity = i.Quantity,
                    Customization = i.Customization
                }).ToList()
            };
        }

        public async Task<int> GetOrderCountForRestaurantAsync(int restaurantId)
        {
            return await _orderRepo.CountByRestaurantIdAsync(restaurantId);
        }

        public async Task<List<OrderResponseDto>> GetOrdersByAgentAsync(int agentId)
        {
            var orders = await _orderRepo.GetByAgentIdAsync(agentId);
            // Only return orders that are still active (i.e., not completed or cancelled)
            var activeStatuses = new[] { "PLACED", "CONFIRMED", "PREPARING", "PICKED_UP", "CUSTOMER_RECEIVED" };
            return orders
                .Where(o => activeStatuses.Contains(o.OrderStatus))
                .Select(MapToResponse)
                .ToList();
        }

        // ─── Helpers ─────────────────────────────────────────────────────────

        /// <summary>
        /// Calls the Payment-Service to find the payment for this order and initiate a refund.
        /// Fails silently with a warning log so order cancellation always succeeds.
        /// </summary>
        private async Task TriggerRefundAsync(int orderId)
        {
            try
            {
                var paymentBaseUrl = _config["ServiceUrls:Payment"] ?? "http://localhost:5236";
                var client = _httpClientFactory.CreateClient();

                // Step 1: Get payment record for this order
                var paymentResponse = await client.GetAsync($"{paymentBaseUrl}/api/v1/payments/order/{orderId}");
                if (!paymentResponse.IsSuccessStatusCode)
                {
                    _logger.LogWarning("No payment found for Order {OrderId} — skipping refund", orderId);
                    return;
                }

                var payment = await paymentResponse.Content.ReadFromJsonAsync<dynamic>();
                if (payment == null) return;

                int paymentId = (int)payment.GetProperty("paymentId").GetInt32();
                string status = payment.GetProperty("status").GetString() ?? "";

                if (status != "PAID") 
                {
                    _logger.LogInformation("Payment {PaymentId} is not PAID (status: {Status}) — skipping refund", paymentId, status);
                    return;
                }

                // Step 2: Trigger refund
                var refundResponse = await client.PutAsync($"{paymentBaseUrl}/api/v1/payments/{paymentId}/refund", null);
                if (refundResponse.IsSuccessStatusCode)
                    _logger.LogInformation("Refund successfully initiated for Payment {PaymentId} (Order {OrderId})", paymentId, orderId);
                else
                    _logger.LogWarning("Refund call failed for Payment {PaymentId} — Status: {Status}", paymentId, refundResponse.StatusCode);
            }
            catch (Exception ex)
            {
                // Never block cancellation due to refund failure — log and continue
                _logger.LogError(ex, "Exception while triggering refund for Order {OrderId}", orderId);
            }
        }

        private static OrderResponseDto MapToResponse(Entities.Order order)
        {
            return new OrderResponseDto
            {
                OrderId = order.OrderId,
                CustomerId = order.CustomerId,
                RestaurantId = order.RestaurantId,
                DeliveryAgentId = order.DeliveryAgentId,
                TotalAmount = order.TotalAmount,
                Discount = order.Discount,
                FinalAmount = order.FinalAmount,
                ModeOfPayment = order.ModeOfPayment,
                OrderStatus = order.OrderStatus,
                OrderDate = order.OrderDate,
                EstimatedDelivery = order.EstimatedDelivery,
                DeliveryAddress = order.DeliveryAddress,
                SpecialInstructions = order.SpecialInstructions,
                Items = order.Items.Select(i => new OrderItemResponseDto
                {
                    OrderItemId = i.OrderItemId,
                    MenuItemId = i.MenuItemId,
                    Name = i.Name,
                    Price = i.Price,
                    Quantity = i.Quantity,
                    Customization = i.Customization,
                    SubTotal = i.Price * i.Quantity
                }).ToList()
            };
        }
    }
}
