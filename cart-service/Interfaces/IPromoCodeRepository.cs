using QuickBite.Cart.Entities;

namespace QuickBite.Cart.Interfaces
{
    public interface IPromoCodeRepository
    {
        Task<PromoCode?> GetByCodeAsync(string code);
        Task UpdateAsync(PromoCode promoCode);
    }
}
