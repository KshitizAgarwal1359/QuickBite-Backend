using QuickBite.Menu.Entities;

namespace QuickBite.Menu.Interfaces
{
    public interface IMenuCategoryRepository
    {
        Task<MenuCategory?> GetByIdAsync(int categoryId);
        Task<List<MenuCategory>> GetByRestaurantIdAsync(int restaurantId);
        Task<MenuCategory> AddAsync(MenuCategory category);
        Task UpdateAsync(MenuCategory category);
        Task DeleteAsync(int categoryId);
    }
}
