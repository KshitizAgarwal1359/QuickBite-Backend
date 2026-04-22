using QuickBite.Cart.DTOs;
using QuickBite.Cart.Entities;
using QuickBite.Cart.Interfaces;

namespace QuickBite.Cart.Services
{
    public class CartServiceImpl : ICartService
    {
        private readonly ICartRepository _cartRepo;
        private readonly IPromoCodeRepository _promoRepo;
        private readonly ILogger<CartServiceImpl> _logger;

        public CartServiceImpl(ICartRepository cartRepo, IPromoCodeRepository promoRepo, ILogger<CartServiceImpl> logger)
        {
            _cartRepo = cartRepo;
            _promoRepo = promoRepo;
            _logger = logger;
        }

        public async Task<CartResponseDto> GetCartByCustomerAsync(int customerId)
        {
            var cart = await _cartRepo.GetByCustomerIdAsync(customerId);
            if (cart == null)
                throw new KeyNotFoundException($"No active cart found for customer {customerId}");

            return MapCartToResponse(cart);
        }

        public async Task<CartResponseDto> AddItemAsync(int customerId, AddItemRequestDto request)
        {
            var cart = await _cartRepo.GetByCustomerIdAsync(customerId);

            if (cart == null)
            {
                // Create new cart
                cart = new Entities.Cart
                {
                    CustomerId = customerId,
                    RestaurantId = request.RestaurantId,
                    TotalPrice = 0,
                    DiscountAmount = 0,
                    CreatedAt = DateTime.UtcNow
                };
                cart = await _cartRepo.AddAsync(cart);
                _logger.LogInformation("New cart created for CustomerId {CustomerId}, RestaurantId {RestaurantId}", customerId, request.RestaurantId);
            }
            else if (cart.RestaurantId != request.RestaurantId)
            {
                // Different restaurant — enforce single-restaurant rule
                throw new InvalidOperationException(
                    $"Cart is tied to RestaurantId {cart.RestaurantId}. Clear the cart first to add items from RestaurantId {request.RestaurantId}.");
            }

            // Check if item already exists in cart (same MenuItemId)
            var existingItem = cart.Items.FirstOrDefault(i => i.MenuItemId == request.MenuItemId);

            if (existingItem != null)
            {
                existingItem.Quantity += request.Quantity;
                _logger.LogInformation("Item {MenuItemId} quantity increased to {Qty} in CartId {CartId}",
                    request.MenuItemId, existingItem.Quantity, cart.CartId);
            }
            else
            {
                // Snapshot price and name
                var newItem = new CartItem
                {
                    CartId = cart.CartId,
                    MenuItemId = request.MenuItemId,
                    Name = request.Name,
                    Price = request.Price,
                    Quantity = request.Quantity,
                    Customization = request.Customization
                };
                cart.Items.Add(newItem);
                _logger.LogInformation("Item '{Name}' (MenuItemId {MenuItemId}) added to CartId {CartId} | Price snapshot: {Price}",
                    request.Name, request.MenuItemId, cart.CartId, request.Price);
            }

            RecalculateTotal(cart);
            await _cartRepo.UpdateAsync(cart);
            return MapCartToResponse(cart);
        }

        public async Task<CartResponseDto> RemoveItemAsync(int cartId, int itemId, int customerId)
        {
            var cart = await _cartRepo.GetByCartIdAsync(cartId);
            if (cart == null)
                throw new KeyNotFoundException($"Cart with ID {cartId} not found");

            if (cart.CustomerId != customerId)
                throw new UnauthorizedAccessException("You do not have permission to modify this cart");

            var item = cart.Items.FirstOrDefault(i => i.ItemId == itemId);
            if (item == null)
                throw new KeyNotFoundException($"Item with ID {itemId} not found in cart");

            cart.Items.Remove(item);
            _logger.LogInformation("Item '{Name}' (ItemId {ItemId}) removed from CartId {CartId}", item.Name, itemId, cartId);

            RecalculateTotal(cart);
            await _cartRepo.UpdateAsync(cart);
            return MapCartToResponse(cart);
        }

        public async Task<CartResponseDto> UpdateQuantityAsync(int customerId, UpdateQuantityRequestDto request)
        {
            var cart = await _cartRepo.GetByCartIdAsync(request.CartId);
            if (cart == null)
                throw new KeyNotFoundException($"Cart with ID {request.CartId} not found");

            if (cart.CustomerId != customerId)
                throw new UnauthorizedAccessException("You do not have permission to modify this cart");

            var item = cart.Items.FirstOrDefault(i => i.ItemId == request.ItemId);
            if (item == null)
                throw new KeyNotFoundException($"Item with ID {request.ItemId} not found in cart");

            if (request.Quantity == 0)
            {
                cart.Items.Remove(item);
                _logger.LogInformation("Item '{Name}' removed from CartId {CartId} (qty set to 0)", item.Name, request.CartId);
            }
            else
            {
                var oldQty = item.Quantity;
                item.Quantity = request.Quantity;
                _logger.LogInformation("Item '{Name}' qty updated: {Old} → {New} in CartId {CartId}",
                    item.Name, oldQty, request.Quantity, request.CartId);
            }

            RecalculateTotal(cart);
            await _cartRepo.UpdateAsync(cart);
            return MapCartToResponse(cart);
        }

        public async Task ClearCartAsync(int cartId, int customerId)
        {
            var cart = await _cartRepo.GetByCartIdAsync(cartId);
            if (cart == null)
                throw new KeyNotFoundException($"Cart with ID {cartId} not found");

            if (cart.CustomerId != customerId)
                throw new UnauthorizedAccessException("You do not have permission to clear this cart");

            var itemCount = cart.Items.Count;
            await _cartRepo.DeleteAsync(cartId);
            _logger.LogWarning("CartId {CartId} cleared for CustomerId {CustomerId} — {ItemCount} items removed", cartId, customerId, itemCount);
        }

        public async Task<CartResponseDto> ApplyPromoCodeAsync(int customerId, ApplyPromoRequestDto request)
        {
            var cart = await _cartRepo.GetByCartIdAsync(request.CartId);
            if (cart == null)
                throw new KeyNotFoundException($"Cart with ID {request.CartId} not found");

            if (cart.CustomerId != customerId)
                throw new UnauthorizedAccessException("You do not have permission to modify this cart");

            if (cart.Items.Count == 0)
                throw new InvalidOperationException("Cannot apply promo code to an empty cart");

            // Lookup promo code
            var promo = await _promoRepo.GetByCodeAsync(request.PromoCode);
            if (promo == null)
                throw new KeyNotFoundException($"Promo code '{request.PromoCode}' not found");

            // Validate
            if (!promo.IsActive)
                throw new InvalidOperationException("This promo code is no longer active");

            if (promo.ExpiryDate < DateTime.UtcNow)
                throw new InvalidOperationException("This promo code has expired");

            if (promo.TimesUsed >= promo.UsageLimit)
                throw new InvalidOperationException("This promo code has reached its usage limit");

            if (cart.TotalPrice < promo.MinOrderValue)
                throw new InvalidOperationException($"Minimum order value of ₹{promo.MinOrderValue} required to use this promo code. Current total: ₹{cart.TotalPrice}");

            // Calculate discount
            double discount = cart.TotalPrice * promo.DiscountPercent / 100.0;
            if (discount > promo.MaxDiscountAmount)
                discount = promo.MaxDiscountAmount;

            discount = Math.Round(discount, 2);

            cart.DiscountAmount = discount;
            cart.PromoCode = promo.Code;

            // Increment usage
            promo.TimesUsed++;
            await _promoRepo.UpdateAsync(promo);
            await _cartRepo.UpdateAsync(cart);

            _logger.LogInformation("Promo '{Code}' applied to CartId {CartId} — Discount: ₹{Discount} (Total: ₹{Total}, Final: ₹{Final})",
                promo.Code, cart.CartId, discount, cart.TotalPrice, cart.TotalPrice - discount);

            return MapCartToResponse(cart);
        }

        public async Task<List<CartResponseDto>> GetAllCartsAsync()
        {
            var carts = await _cartRepo.GetAllAsync();
            return carts.Select(MapCartToResponse).ToList();
        }

        // ─── Helpers ──────────────────────────────────────────────────────────────

        private static void RecalculateTotal(Entities.Cart cart)
        {
            cart.TotalPrice = Math.Round(cart.Items.Sum(i => i.Price * i.Quantity), 2);

            // Reset discount if total changed (promo may no longer be valid)
            if (cart.DiscountAmount > 0)
            {
                cart.DiscountAmount = 0;
                cart.PromoCode = null;
            }
        }

        private static CartResponseDto MapCartToResponse(Entities.Cart cart)
        {
            return new CartResponseDto
            {
                CartId = cart.CartId,
                CustomerId = cart.CustomerId,
                RestaurantId = cart.RestaurantId,
                TotalPrice = cart.TotalPrice,
                DiscountAmount = cart.DiscountAmount,
                PromoCode = cart.PromoCode,
                FinalPrice = Math.Round(cart.TotalPrice - cart.DiscountAmount, 2),
                CreatedAt = cart.CreatedAt,
                Items = cart.Items.Select(i => new CartItemResponseDto
                {
                    ItemId = i.ItemId,
                    MenuItemId = i.MenuItemId,
                    Name = i.Name,
                    Price = i.Price,
                    Quantity = i.Quantity,
                    Customization = i.Customization,
                    SubTotal = Math.Round(i.Price * i.Quantity, 2)
                }).ToList()
            };
        }
    }
}
