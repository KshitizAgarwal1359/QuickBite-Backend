using QuickBite.Menu.DTOs;
using QuickBite.Menu.Entities;
using QuickBite.Menu.Interfaces;

namespace QuickBite.Menu.Services
{
    public class MenuServiceImpl : IMenuService
    {
        private readonly IMenuCategoryRepository _categoryRepo;
        private readonly IMenuItemRepository _itemRepo;
        private readonly ILogger<MenuServiceImpl> _logger;

        public MenuServiceImpl(
            IMenuCategoryRepository categoryRepo,
            IMenuItemRepository itemRepo,
            ILogger<MenuServiceImpl> logger)
        {
            _categoryRepo = categoryRepo;
            _itemRepo = itemRepo;
            _logger = logger;
        }

        // ─── Categories ──────────────────────────────────────────────────────────

        public async Task<CategoryResponseDto> AddCategoryAsync(AddCategoryRequestDto request)
        {
            var category = new MenuCategory
            {
                RestaurantId = request.RestaurantId,
                Name = request.Name,
                Description = request.Description,
                ImageUrl = request.ImageUrl,
                DisplayOrder = request.DisplayOrder
            };

            var created = await _categoryRepo.AddAsync(category);
            _logger.LogInformation("Category '{Name}' added for RestaurantId {RestaurantId}", created.Name, created.RestaurantId);
            return MapCategoryToResponse(created);
        }

        public async Task<CategoryResponseDto> UpdateCategoryAsync(int categoryId, UpdateCategoryRequestDto request)
        {
            var category = await _categoryRepo.GetByIdAsync(categoryId);
            if (category == null)
                throw new KeyNotFoundException($"Category with ID {categoryId} not found");

            if (!string.IsNullOrWhiteSpace(request.Name)) category.Name = request.Name;
            if (request.Description != null) category.Description = request.Description;
            if (request.ImageUrl != null) category.ImageUrl = request.ImageUrl;
            if (request.DisplayOrder.HasValue) category.DisplayOrder = request.DisplayOrder.Value;

            await _categoryRepo.UpdateAsync(category);
            _logger.LogInformation("Category {CategoryId} updated", categoryId);
            return MapCategoryToResponse(category);
        }

        public async Task DeleteCategoryAsync(int categoryId)
        {
            var category = await _categoryRepo.GetByIdAsync(categoryId);
            if (category == null)
                throw new KeyNotFoundException($"Category with ID {categoryId} not found");

            var itemCount = category.Items.Count;
            await _categoryRepo.DeleteAsync(categoryId);
            _logger.LogWarning("Category {CategoryId} '{Name}' deleted with {ItemCount} items (cascade)", categoryId, category.Name, itemCount);
        }

        public async Task<List<CategoryResponseDto>> GetCategoriesByRestaurantAsync(int restaurantId)
        {
            var categories = await _categoryRepo.GetByRestaurantIdAsync(restaurantId);
            return categories.Select(MapCategoryToResponse).ToList();
        }

        // ─── Full Menu (nested) ──────────────────────────────────────────────────

        public async Task<List<CategoryResponseDto>> GetMenuByRestaurantAsync(int restaurantId)
        {
            var categories = await _categoryRepo.GetByRestaurantIdAsync(restaurantId);

            return categories.Select(c => new CategoryResponseDto
            {
                CategoryId = c.CategoryId,
                RestaurantId = c.RestaurantId,
                Name = c.Name,
                Description = c.Description,
                ImageUrl = c.ImageUrl,
                DisplayOrder = c.DisplayOrder,
                Items = c.Items
                    .Where(i => i.IsAvailable)  // Only show available items to customers
                    .Select(MapItemToResponse)
                    .ToList()
            }).ToList();
        }

        // ─── Menu Items ──────────────────────────────────────────────────────────

        public async Task<MenuItemResponseDto> AddMenuItemAsync(AddMenuItemRequestDto request)
        {
            // Verify category exists
            var category = await _categoryRepo.GetByIdAsync(request.CategoryId);
            if (category == null)
                throw new KeyNotFoundException($"Category with ID {request.CategoryId} not found");

            // If discounted price not set or higher than price, default to base price
            if (request.DiscountedPrice <= 0 || request.DiscountedPrice > request.Price)
                request.DiscountedPrice = request.Price;

            var item = new MenuItem
            {
                RestaurantId = request.RestaurantId,
                CategoryId = request.CategoryId,
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                DiscountedPrice = request.DiscountedPrice,
                ImageUrl = request.ImageUrl,
                IsVeg = request.IsVeg,
                IsAvailable = true,
                Rating = 0,
                Calories = request.Calories,
                Tags = request.Tags
            };

            var created = await _itemRepo.AddAsync(item);
            _logger.LogInformation(
                "MenuItem '{Name}' added to CategoryId {CategoryId}, RestaurantId {RestaurantId} | Price: {Price}, Veg: {IsVeg}",
                created.Name, created.CategoryId, created.RestaurantId, created.Price, created.IsVeg);
            return MapItemToResponse(created);
        }

        public async Task<MenuItemResponseDto> GetItemByIdAsync(int itemId)
        {
            var item = await _itemRepo.GetByItemIdAsync(itemId);
            if (item == null)
                throw new KeyNotFoundException($"Menu item with ID {itemId} not found");
            return MapItemToResponse(item);
        }

        public async Task<MenuItemResponseDto> UpdateMenuItemAsync(int itemId, UpdateMenuItemRequestDto request)
        {
            var item = await _itemRepo.GetByItemIdAsync(itemId);
            if (item == null)
                throw new KeyNotFoundException($"Menu item with ID {itemId} not found");

            var changes = new List<string>();

            if (!string.IsNullOrWhiteSpace(request.Name) && request.Name != item.Name)
            { item.Name = request.Name; changes.Add("Name"); }

            if (request.Description != null && request.Description != item.Description)
            { item.Description = request.Description; changes.Add("Description"); }

            if (request.Price.HasValue && request.Price.Value != item.Price)
            { item.Price = request.Price.Value; changes.Add("Price"); }

            if (request.DiscountedPrice.HasValue && request.DiscountedPrice.Value != item.DiscountedPrice)
            { item.DiscountedPrice = request.DiscountedPrice.Value; changes.Add("DiscountedPrice"); }

            if (request.ImageUrl != null && request.ImageUrl != item.ImageUrl)
            { item.ImageUrl = request.ImageUrl; changes.Add("ImageUrl"); }

            if (request.IsVeg.HasValue && request.IsVeg.Value != item.IsVeg)
            { item.IsVeg = request.IsVeg.Value; changes.Add("IsVeg"); }

            if (request.Calories.HasValue && request.Calories.Value != item.Calories)
            { item.Calories = request.Calories.Value; changes.Add("Calories"); }

            if (request.Tags != null && request.Tags != item.Tags)
            { item.Tags = request.Tags; changes.Add("Tags"); }

            if (changes.Count > 0)
            {
                await _itemRepo.UpdateAsync(item);
                _logger.LogInformation("MenuItem {ItemId} updated. Changed: {Fields}", itemId, string.Join(", ", changes));
            }

            return MapItemToResponse(item);
        }

        public async Task<MenuItemResponseDto> ToggleAvailabilityAsync(int itemId)
        {
            var item = await _itemRepo.GetByItemIdAsync(itemId);
            if (item == null)
                throw new KeyNotFoundException($"Menu item with ID {itemId} not found");

            item.IsAvailable = !item.IsAvailable;
            await _itemRepo.UpdateAsync(item);

            _logger.LogInformation("MenuItem {ItemId} '{Name}' toggled to {Status}",
                itemId, item.Name, item.IsAvailable ? "AVAILABLE" : "OUT-OF-STOCK");

            return MapItemToResponse(item);
        }

        public async Task DeleteMenuItemAsync(int itemId)
        {
            var item = await _itemRepo.GetByItemIdAsync(itemId);
            if (item == null)
                throw new KeyNotFoundException($"Menu item with ID {itemId} not found");

            await _itemRepo.DeleteAsync(itemId);
            _logger.LogWarning("MenuItem {ItemId} '{Name}' deleted", itemId, item.Name);
        }

        public async Task<List<MenuItemResponseDto>> SearchMenuItemsAsync(string keyword)
        {
            var items = await _itemRepo.SearchByNameAsync(keyword);
            return items.Select(MapItemToResponse).ToList();
        }

        public async Task<List<MenuItemResponseDto>> GetVegItemsAsync(int restaurantId)
        {
            var items = await _itemRepo.GetByIsVegAndRestaurantIdAsync(true, restaurantId);
            return items.Select(MapItemToResponse).ToList();
        }

        // ─── Helpers ──────────────────────────────────────────────────────────────

        private static CategoryResponseDto MapCategoryToResponse(MenuCategory category)
        {
            return new CategoryResponseDto
            {
                CategoryId = category.CategoryId,
                RestaurantId = category.RestaurantId,
                Name = category.Name,
                Description = category.Description,
                ImageUrl = category.ImageUrl,
                DisplayOrder = category.DisplayOrder,
                Items = category.Items.Select(MapItemToResponse).ToList()
            };
        }

        private static MenuItemResponseDto MapItemToResponse(MenuItem item)
        {
            return new MenuItemResponseDto
            {
                ItemId = item.ItemId,
                RestaurantId = item.RestaurantId,
                CategoryId = item.CategoryId,
                Name = item.Name,
                Description = item.Description,
                Price = item.Price,
                DiscountedPrice = item.DiscountedPrice,
                ImageUrl = item.ImageUrl,
                IsVeg = item.IsVeg,
                IsAvailable = item.IsAvailable,
                Rating = item.Rating,
                Calories = item.Calories,
                Tags = item.Tags
            };
        }
    }
}
