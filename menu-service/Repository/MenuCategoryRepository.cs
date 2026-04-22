using Microsoft.EntityFrameworkCore;
using QuickBite.Menu.Data;
using QuickBite.Menu.Entities;
using QuickBite.Menu.Interfaces;

namespace QuickBite.Menu.Repository
{
    public class MenuCategoryRepository : IMenuCategoryRepository
    {
        private readonly MenuDbContext _context;

        public MenuCategoryRepository(MenuDbContext context)
        {
            _context = context;
        }

        public async Task<MenuCategory?> GetByIdAsync(int categoryId)
        {
            return await _context.MenuCategories
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.CategoryId == categoryId);
        }

        public async Task<List<MenuCategory>> GetByRestaurantIdAsync(int restaurantId)
        {
            return await _context.MenuCategories
                .Where(c => c.RestaurantId == restaurantId)
                .OrderBy(c => c.DisplayOrder)
                .Include(c => c.Items)
                .ToListAsync();
        }

        public async Task<MenuCategory> AddAsync(MenuCategory category)
        {
            await _context.MenuCategories.AddAsync(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task UpdateAsync(MenuCategory category)
        {
            _context.MenuCategories.Update(category);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int categoryId)
        {
            var category = await _context.MenuCategories.FindAsync(categoryId);
            if (category != null)
            {
                _context.MenuCategories.Remove(category);
                await _context.SaveChangesAsync();
            }
        }
    }
}
