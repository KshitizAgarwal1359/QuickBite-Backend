using Microsoft.EntityFrameworkCore;
using QuickBite.Cart.Data;
using QuickBite.Cart.Interfaces;

namespace QuickBite.Cart.Repository
{
    public class CartRepository : ICartRepository
    {
        private readonly CartDbContext _context;

        public CartRepository(CartDbContext context) { _context = context; }

        public async Task<Entities.Cart?> GetByCustomerIdAsync(int customerId)
        {
            return await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);
        }

        public async Task<Entities.Cart?> GetByCartIdAsync(int cartId)
        {
            return await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.CartId == cartId);
        }

        public async Task<bool> ExistsByCustomerIdAsync(int customerId)
        {
            return await _context.Carts.AnyAsync(c => c.CustomerId == customerId);
        }

        public async Task<List<Entities.Cart>> GetByRestaurantIdAsync(int restaurantId)
        {
            return await _context.Carts
                .Where(c => c.RestaurantId == restaurantId)
                .Include(c => c.Items)
                .ToListAsync();
        }

        public async Task<List<Entities.Cart>> GetAllAsync()
        {
            return await _context.Carts
                .Include(c => c.Items)
                .ToListAsync();
        }

        public async Task<Entities.Cart> AddAsync(Entities.Cart cart)
        {
            await _context.Carts.AddAsync(cart);
            await _context.SaveChangesAsync();
            return cart;
        }

        public async Task UpdateAsync(Entities.Cart cart)
        {
            _context.Carts.Update(cart);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int cartId)
        {
            var cart = await _context.Carts.FindAsync(cartId);
            if (cart != null)
            {
                _context.Carts.Remove(cart);
                await _context.SaveChangesAsync();
            }
        }
    }
}
