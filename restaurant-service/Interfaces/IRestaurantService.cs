using QuickBite.Restaurant.DTOs;

namespace QuickBite.Restaurant.Interfaces
{
    public interface IRestaurantService
    {
        Task<RestaurantResponseDto> RegisterRestaurantAsync(int ownerId, RegisterRestaurantRequestDto request);
        Task<RestaurantResponseDto> GetByIdAsync(int restaurantId);
        Task<List<RestaurantResponseDto>> GetByOwnerAsync(int ownerId);
        Task<List<RestaurantResponseDto>> GetByCuisineAsync(string cuisine);
        Task<List<RestaurantResponseDto>> GetByCityAsync(string city);
        Task<List<RestaurantResponseDto>> GetAllAsync();
        Task<List<RestaurantResponseDto>> GetNearbyAsync(double latitude, double longitude, double radiusInKm);
        Task<List<RestaurantResponseDto>> SearchRestaurantsAsync(string keyword);
        Task<RestaurantResponseDto> UpdateRestaurantAsync(int restaurantId, int callerId, string callerRole, UpdateRestaurantRequestDto request);
        Task<RestaurantResponseDto> ApproveRestaurantAsync(int restaurantId);
        Task<RestaurantResponseDto> ToggleIsOpenAsync(int restaurantId, int callerId);
        Task DeleteRestaurantAsync(int restaurantId);
        Task<RestaurantResponseDto> UpdateRatingAsync(int restaurantId, double avgRating);
    }
}
