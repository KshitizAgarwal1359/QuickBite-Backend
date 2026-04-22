using Microsoft.EntityFrameworkCore;
using QuickBite.Cart.Data;
using QuickBite.Cart.Entities;
using QuickBite.Cart.Interfaces;

namespace QuickBite.Cart.Repository
{
    public class PromoCodeRepository : IPromoCodeRepository
    {
        private readonly CartDbContext _context;

        public PromoCodeRepository(CartDbContext context) { _context = context; }

        public async Task<PromoCode?> GetByCodeAsync(string code)
        {
            return await _context.PromoCodes
                .FirstOrDefaultAsync(p => p.Code.ToUpper() == code.ToUpper());
        }

        public async Task UpdateAsync(PromoCode promoCode)
        {
            _context.PromoCodes.Update(promoCode);
            await _context.SaveChangesAsync();
        }
    }
}
