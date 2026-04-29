using Microsoft.EntityFrameworkCore;
using QuickBite.Restaurant.Data;
using QuickBite.Restaurant.Interfaces;

namespace QuickBite.Restaurant.Repository
{
    public class RestaurantRepository : IRestaurantRepository
    {
        private readonly RestaurantDbContext _context;

        public RestaurantRepository(RestaurantDbContext context)
        {
            _context = context;
        }

        public async Task<Entities.Restaurant?> GetByIdAsync(int restaurantId)
        {
            return await _context.Restaurants.FindAsync(restaurantId);
        }

        public async Task<List<Entities.Restaurant>> FindByOwnerIdAsync(int ownerId)
        {
            return await _context.Restaurants
                .Where(r => r.OwnerId == ownerId)
                .ToListAsync();
        }

        public async Task<List<Entities.Restaurant>> FindByCuisineAsync(string cuisine)
        {
            return await _context.Restaurants
                .Where(r => r.Cuisine.ToLower() == cuisine.ToLower() && r.IsApproved)
                .ToListAsync();
        }

        public async Task<List<Entities.Restaurant>> FindByCityAsync(string city)
        {
            return await _context.Restaurants
                .Where(r => r.City.ToLower() == city.ToLower() && r.IsApproved)
                .ToListAsync();
        }

        public async Task<List<Entities.Restaurant>> FindByIsOpenAndIsApprovedAsync(bool isOpen, bool isApproved)
        {
            return await _context.Restaurants
                .Where(r => r.IsOpen == isOpen && r.IsApproved == isApproved)
                .ToListAsync();
        }

        public async Task<List<Entities.Restaurant>> SearchByNameAsync(string keyword)
        {
            var lower = keyword.ToLower();
            return await _context.Restaurants
                .Where(r => r.IsApproved &&
                    (r.Name.ToLower().Contains(lower) ||
                     r.Cuisine.ToLower().Contains(lower) ||
                     (r.Description != null && r.Description.ToLower().Contains(lower))))
                .ToListAsync();
        }

        public async Task<List<Entities.Restaurant>> GetAllAsync()
        {
            return await _context.Restaurants.ToListAsync();
        }

        /// <summary>
        /// Finds nearby restaurants using the Haversine formula.
        /// Earth radius = 6371 km.
        /// Returns all approved restaurants within the given radius.
        /// </summary>
        public async Task<List<Entities.Restaurant>> FindNearbyAsync(double latitude, double longitude, double radiusInKm)
        {
            const double earthRadiusKm = 6371.0;

            // Convert search point to radians
            double latRad = latitude * Math.PI / 180.0;
            double lngRad = longitude * Math.PI / 180.0;

            // Fetch all approved restaurants, then calculate distance in memory
            var approvedRestaurants = await _context.Restaurants
                .Where(r => r.IsApproved)
                .ToListAsync();

            var nearbyRestaurants = approvedRestaurants
                .Select(r =>
                {
                    double rLatRad = r.Latitude * Math.PI / 180.0;
                    double rLngRad = r.Longitude * Math.PI / 180.0;

                    double dLat = rLatRad - latRad;
                    double dLng = rLngRad - lngRad;

                    double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                               Math.Cos(latRad) * Math.Cos(rLatRad) *
                               Math.Sin(dLng / 2) * Math.Sin(dLng / 2);

                    double c = 2 * Math.Asin(Math.Sqrt(a));
                    double distance = earthRadiusKm * c;

                    return new { Restaurant = r, Distance = distance };
                })
                .Where(x => x.Distance <= radiusInKm)
                .OrderBy(x => x.Distance)
                .Select(x => x.Restaurant)
                .ToList();

            return nearbyRestaurants;
        }

        public async Task<int> CountByCityAsync(string city)
        {
            return await _context.Restaurants
                .CountAsync(r => r.City.ToLower() == city.ToLower() && r.IsApproved);
        }

        public async Task<Entities.Restaurant> AddAsync(Entities.Restaurant restaurant)
        {
            await _context.Restaurants.AddAsync(restaurant);
            await _context.SaveChangesAsync();
            return restaurant;
        }

        public async Task UpdateAsync(Entities.Restaurant restaurant)
        {
            _context.Restaurants.Update(restaurant);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int restaurantId)
        {
            var restaurant = await _context.Restaurants.FindAsync(restaurantId);
            if (restaurant != null)
            {
                _context.Restaurants.Remove(restaurant);
                await _context.SaveChangesAsync();
            }
        }
    }
}
