using QuickBite.Restaurant.DTOs;
using QuickBite.Restaurant.Interfaces;

namespace QuickBite.Restaurant.Services
{
    public class RestaurantServiceImpl : IRestaurantService
    {
        private readonly IRestaurantRepository _repository;
        private readonly ILogger<RestaurantServiceImpl> _logger;

        public RestaurantServiceImpl(IRestaurantRepository repository, ILogger<RestaurantServiceImpl> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<RestaurantResponseDto> RegisterRestaurantAsync(int ownerId, RegisterRestaurantRequestDto request)
        {
            var restaurant = new Entities.Restaurant
            {
                OwnerId = ownerId,
                Name = request.Name,
                Description = request.Description,
                Cuisine = request.Cuisine,
                Address = request.Address,
                City = request.City,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                Phone = request.Phone,
                DeliveryRadius = request.DeliveryRadius,
                MinOrderAmount = request.MinOrderAmount,
                EstimatedDeliveryMin = request.EstimatedDeliveryMin,
                AvgRating = 0,
                IsOpen = false,
                IsApproved = false
            };

            var created = await _repository.AddAsync(restaurant);

            _logger.LogInformation(
                "Restaurant registered: '{Name}' by OwnerId {OwnerId}, RestaurantId {RestaurantId} — pending approval",
                created.Name, ownerId, created.RestaurantId);

            return MapToResponse(created);
        }

        public async Task<RestaurantResponseDto> GetByIdAsync(int restaurantId)
        {
            var restaurant = await _repository.GetByIdAsync(restaurantId);
            if (restaurant == null)
                throw new KeyNotFoundException($"Restaurant with ID {restaurantId} not found");

            return MapToResponse(restaurant);
        }

        public async Task<List<RestaurantResponseDto>> GetByOwnerAsync(int ownerId)
        {
            var restaurants = await _repository.FindByOwnerIdAsync(ownerId);
            return restaurants.Select(MapToResponse).ToList();
        }

        public async Task<List<RestaurantResponseDto>> GetByCuisineAsync(string cuisine)
        {
            var restaurants = await _repository.FindByCuisineAsync(cuisine);
            return restaurants.Select(MapToResponse).ToList();
        }

        public async Task<List<RestaurantResponseDto>> GetByCityAsync(string city)
        {
            var restaurants = await _repository.FindByCityAsync(city);
            return restaurants.Select(MapToResponse).ToList();
        }

        public async Task<List<RestaurantResponseDto>> GetNearbyAsync(double latitude, double longitude, double radiusInKm)
        {
            var restaurants = await _repository.FindNearbyAsync(latitude, longitude, radiusInKm);

            _logger.LogInformation(
                "Nearby search: lat={Latitude}, lng={Longitude}, radius={Radius}km — found {Count} restaurants",
                latitude, longitude, radiusInKm, restaurants.Count);

            return restaurants.Select(MapToResponse).ToList();
        }

        public async Task<List<RestaurantResponseDto>> SearchRestaurantsAsync(string keyword)
        {
            var restaurants = await _repository.SearchByNameAsync(keyword);
            return restaurants.Select(MapToResponse).ToList();
        }

        public async Task<List<RestaurantResponseDto>> GetAllAsync()
        {
            var restaurants = await _repository.GetAllAsync();
            return restaurants.Select(MapToResponse).ToList();
        }

        public async Task<RestaurantResponseDto> UpdateRestaurantAsync(int restaurantId, int callerId, string callerRole, UpdateRestaurantRequestDto request)
        {
            var restaurant = await _repository.GetByIdAsync(restaurantId);
            if (restaurant == null)
                throw new KeyNotFoundException($"Restaurant with ID {restaurantId} not found");

            // Only the owner or an admin can update
            if (restaurant.OwnerId != callerId && callerRole != "ADMIN")
                throw new UnauthorizedAccessException("You do not have permission to update this restaurant");

            // Track changes for logging
            var changes = new List<string>();

            if (!string.IsNullOrWhiteSpace(request.Name) && request.Name != restaurant.Name)
            { restaurant.Name = request.Name; changes.Add("Name"); }

            if (!string.IsNullOrWhiteSpace(request.Description) && request.Description != restaurant.Description)
            { restaurant.Description = request.Description; changes.Add("Description"); }

            if (!string.IsNullOrWhiteSpace(request.Cuisine) && request.Cuisine != restaurant.Cuisine)
            { restaurant.Cuisine = request.Cuisine; changes.Add("Cuisine"); }

            if (!string.IsNullOrWhiteSpace(request.Address) && request.Address != restaurant.Address)
            { restaurant.Address = request.Address; changes.Add("Address"); }

            if (!string.IsNullOrWhiteSpace(request.City) && request.City != restaurant.City)
            { restaurant.City = request.City; changes.Add("City"); }

            if (request.Latitude.HasValue && request.Latitude.Value != restaurant.Latitude)
            { restaurant.Latitude = request.Latitude.Value; changes.Add("Latitude"); }

            if (request.Longitude.HasValue && request.Longitude.Value != restaurant.Longitude)
            { restaurant.Longitude = request.Longitude.Value; changes.Add("Longitude"); }

            if (request.Phone != null && request.Phone != restaurant.Phone)
            { restaurant.Phone = request.Phone; changes.Add("Phone"); }

            if (request.DeliveryRadius.HasValue && request.DeliveryRadius.Value != restaurant.DeliveryRadius)
            { restaurant.DeliveryRadius = request.DeliveryRadius.Value; changes.Add("DeliveryRadius"); }

            if (request.MinOrderAmount.HasValue && request.MinOrderAmount.Value != restaurant.MinOrderAmount)
            { restaurant.MinOrderAmount = request.MinOrderAmount.Value; changes.Add("MinOrderAmount"); }

            if (request.EstimatedDeliveryMin.HasValue && request.EstimatedDeliveryMin.Value != restaurant.EstimatedDeliveryMin)
            { restaurant.EstimatedDeliveryMin = request.EstimatedDeliveryMin.Value; changes.Add("EstimatedDeliveryMin"); }

            if (changes.Count > 0)
            {
                await _repository.UpdateAsync(restaurant);
                _logger.LogInformation(
                    "Restaurant {RestaurantId} updated by UserId {CallerId}. Changed: {Fields}",
                    restaurantId, callerId, string.Join(", ", changes));
            }

            return MapToResponse(restaurant);
        }

        public async Task<RestaurantResponseDto> ApproveRestaurantAsync(int restaurantId)
        {
            var restaurant = await _repository.GetByIdAsync(restaurantId);
            if (restaurant == null)
                throw new KeyNotFoundException($"Restaurant with ID {restaurantId} not found");

            restaurant.IsApproved = true;
            await _repository.UpdateAsync(restaurant);

            _logger.LogInformation("Restaurant {RestaurantId} '{Name}' approved by admin", restaurantId, restaurant.Name);

            return MapToResponse(restaurant);
        }

        public async Task<RestaurantResponseDto> ToggleIsOpenAsync(int restaurantId, int callerId)
        {
            var restaurant = await _repository.GetByIdAsync(restaurantId);
            if (restaurant == null)
                throw new KeyNotFoundException($"Restaurant with ID {restaurantId} not found");

            if (restaurant.OwnerId != callerId)
                throw new UnauthorizedAccessException("Only the owner can toggle open/closed status");

            if (!restaurant.IsApproved)
                throw new InvalidOperationException("Cannot open a restaurant that hasn't been approved yet");

            restaurant.IsOpen = !restaurant.IsOpen;
            await _repository.UpdateAsync(restaurant);

            _logger.LogInformation(
                "Restaurant {RestaurantId} '{Name}' toggled to {Status} by OwnerId {OwnerId}",
                restaurantId, restaurant.Name, restaurant.IsOpen ? "OPEN" : "CLOSED", callerId);

            return MapToResponse(restaurant);
        }

        public async Task DeleteRestaurantAsync(int restaurantId)
        {
            var restaurant = await _repository.GetByIdAsync(restaurantId);
            if (restaurant == null)
                throw new KeyNotFoundException($"Restaurant with ID {restaurantId} not found");

            await _repository.DeleteAsync(restaurantId);

            _logger.LogWarning("Restaurant {RestaurantId} '{Name}' deleted by admin", restaurantId, restaurant.Name);
        }

        public async Task<RestaurantResponseDto> UpdateRatingAsync(int restaurantId, double avgRating)
        {
            var restaurant = await _repository.GetByIdAsync(restaurantId);
            if (restaurant == null)
                throw new KeyNotFoundException($"Restaurant with ID {restaurantId} not found");

            var oldRating = restaurant.AvgRating;
            restaurant.AvgRating = Math.Round(avgRating, 2);
            await _repository.UpdateAsync(restaurant);

            _logger.LogInformation(
                "Restaurant {RestaurantId} rating updated: {OldRating} → {NewRating}",
                restaurantId, oldRating, restaurant.AvgRating);

            return MapToResponse(restaurant);
        }

        // ─── Helper ──────────────────────────────────────────────────────────────

        private static RestaurantResponseDto MapToResponse(Entities.Restaurant restaurant)
        {
            return new RestaurantResponseDto
            {
                RestaurantId = restaurant.RestaurantId,
                OwnerId = restaurant.OwnerId,
                Name = restaurant.Name,
                Description = restaurant.Description,
                Cuisine = restaurant.Cuisine,
                Address = restaurant.Address,
                City = restaurant.City,
                Latitude = restaurant.Latitude,
                Longitude = restaurant.Longitude,
                Phone = restaurant.Phone,
                AvgRating = restaurant.AvgRating,
                IsOpen = restaurant.IsOpen,
                IsApproved = restaurant.IsApproved,
                DeliveryRadius = restaurant.DeliveryRadius,
                MinOrderAmount = restaurant.MinOrderAmount,
                EstimatedDeliveryMin = restaurant.EstimatedDeliveryMin
            };
        }
    }
}
