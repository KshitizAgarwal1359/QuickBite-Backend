namespace QuickBite.Restaurant.Interfaces
{
    public interface IRestaurantRepository
    {
        Task<Entities.Restaurant?> GetByIdAsync(int restaurantId);
        Task<List<Entities.Restaurant>> FindByOwnerIdAsync(int ownerId);
        Task<List<Entities.Restaurant>> FindByCuisineAsync(string cuisine);
        Task<List<Entities.Restaurant>> FindByCityAsync(string city);
        Task<List<Entities.Restaurant>> FindByIsOpenAndIsApprovedAsync(bool isOpen, bool isApproved);
        Task<List<Entities.Restaurant>> SearchByNameAsync(string keyword);
        Task<List<Entities.Restaurant>> FindNearbyAsync(double latitude, double longitude, double radiusInKm);
        Task<int> CountByCityAsync(string city);
        Task<Entities.Restaurant> AddAsync(Entities.Restaurant restaurant);
        Task<List<Entities.Restaurant>> GetAllAsync();
        Task UpdateAsync(Entities.Restaurant restaurant);
        Task DeleteAsync(int restaurantId);
    }
}
