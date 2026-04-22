using QuickBite.Cart.DTOs;

namespace QuickBite.Cart.Interfaces
{
    public interface ICartService
    {
        Task<CartResponseDto> GetCartByCustomerAsync(int customerId);
        Task<CartResponseDto> AddItemAsync(int customerId, AddItemRequestDto request);
        Task<CartResponseDto> RemoveItemAsync(int cartId, int itemId, int customerId);
        Task<CartResponseDto> UpdateQuantityAsync(int customerId, UpdateQuantityRequestDto request);
        Task ClearCartAsync(int cartId, int customerId);
        Task<CartResponseDto> ApplyPromoCodeAsync(int customerId, ApplyPromoRequestDto request);
        Task<List<CartResponseDto>> GetAllCartsAsync();
    }
}
