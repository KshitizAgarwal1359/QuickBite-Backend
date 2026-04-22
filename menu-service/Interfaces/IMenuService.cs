using QuickBite.Menu.DTOs;

namespace QuickBite.Menu.Interfaces
{
    public interface IMenuService
    {
        Task<CategoryResponseDto> AddCategoryAsync(AddCategoryRequestDto request);
        Task<MenuItemResponseDto> AddMenuItemAsync(AddMenuItemRequestDto request);
        Task<List<CategoryResponseDto>> GetMenuByRestaurantAsync(int restaurantId);
        Task<List<CategoryResponseDto>> GetCategoriesByRestaurantAsync(int restaurantId);
        Task<MenuItemResponseDto> GetItemByIdAsync(int itemId);
        Task<MenuItemResponseDto> UpdateMenuItemAsync(int itemId, UpdateMenuItemRequestDto request);
        Task<CategoryResponseDto> UpdateCategoryAsync(int categoryId, UpdateCategoryRequestDto request);
        Task<MenuItemResponseDto> ToggleAvailabilityAsync(int itemId);
        Task DeleteMenuItemAsync(int itemId);
        Task DeleteCategoryAsync(int categoryId);
        Task<List<MenuItemResponseDto>> SearchMenuItemsAsync(string keyword);
        Task<List<MenuItemResponseDto>> GetVegItemsAsync(int restaurantId);
    }
}
