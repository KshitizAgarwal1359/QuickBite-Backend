namespace QuickBite.Cart.Interfaces
{
    public interface ICartRepository
    {
        Task<Entities.Cart?> GetByCustomerIdAsync(int customerId);
        Task<Entities.Cart?> GetByCartIdAsync(int cartId);
        Task<bool> ExistsByCustomerIdAsync(int customerId);
        Task<List<Entities.Cart>> GetByRestaurantIdAsync(int restaurantId);
        Task<List<Entities.Cart>> GetAllAsync();
        Task<Entities.Cart> AddAsync(Entities.Cart cart);
        Task UpdateAsync(Entities.Cart cart);
        Task DeleteAsync(int cartId);
    }
}
