using Microsoft.EntityFrameworkCore;
using QuickBite.Menu.Data;
using QuickBite.Menu.Entities;
using QuickBite.Menu.Interfaces;

namespace QuickBite.Menu.Repository
{
    public class MenuItemRepository : IMenuItemRepository
    {
        private readonly MenuDbContext _context;

        public MenuItemRepository(MenuDbContext context)
        {
            _context = context;
        }

        public async Task<MenuItem?> GetByItemIdAsync(int itemId)
        {
            return await _context.MenuItems.FindAsync(itemId);
        }

        public async Task<List<MenuItem>> GetByRestaurantIdAsync(int restaurantId)
        {
            return await _context.MenuItems
                .Where(i => i.RestaurantId == restaurantId)
                .ToListAsync();
        }

        public async Task<List<MenuItem>> GetByCategoryIdAsync(int categoryId)
        {
            return await _context.MenuItems
                .Where(i => i.CategoryId == categoryId)
                .ToListAsync();
        }

        public async Task<List<MenuItem>> GetByIsVegAndRestaurantIdAsync(bool isVeg, int restaurantId)
        {
            return await _context.MenuItems
                .Where(i => i.IsVeg == isVeg && i.RestaurantId == restaurantId && i.IsAvailable)
                .ToListAsync();
        }

        public async Task<List<MenuItem>> SearchByNameAsync(string keyword)
        {
            return await _context.MenuItems
                .Where(i => i.Name.Contains(keyword) && i.IsAvailable)
                .ToListAsync();
        }

        public async Task<List<MenuItem>> GetByIsAvailableAsync(bool isAvailable)
        {
            return await _context.MenuItems
                .Where(i => i.IsAvailable == isAvailable)
                .ToListAsync();
        }

        public async Task<int> CountByRestaurantIdAsync(int restaurantId)
        {
            return await _context.MenuItems
                .CountAsync(i => i.RestaurantId == restaurantId);
        }

        public async Task<MenuItem> AddAsync(MenuItem item)
        {
            await _context.MenuItems.AddAsync(item);
            await _context.SaveChangesAsync();
            return item;
        }

        public async Task UpdateAsync(MenuItem item)
        {
            _context.MenuItems.Update(item);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int itemId)
        {
            var item = await _context.MenuItems.FindAsync(itemId);
            if (item != null)
            {
                _context.MenuItems.Remove(item);
                await _context.SaveChangesAsync();
            }
        }
    }
}
