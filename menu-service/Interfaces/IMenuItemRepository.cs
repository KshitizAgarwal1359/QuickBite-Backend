using QuickBite.Menu.Entities;

namespace QuickBite.Menu.Interfaces
{
    public interface IMenuItemRepository
    {
        Task<MenuItem?> GetByItemIdAsync(int itemId);
        Task<List<MenuItem>> GetByRestaurantIdAsync(int restaurantId);
        Task<List<MenuItem>> GetByCategoryIdAsync(int categoryId);
        Task<List<MenuItem>> GetByIsVegAndRestaurantIdAsync(bool isVeg, int restaurantId);
        Task<List<MenuItem>> SearchByNameAsync(string keyword);
        Task<List<MenuItem>> GetByIsAvailableAsync(bool isAvailable);
        Task<int> CountByRestaurantIdAsync(int restaurantId);
        Task<MenuItem> AddAsync(MenuItem item);
        Task UpdateAsync(MenuItem item);
        Task DeleteAsync(int itemId);
    }
}
