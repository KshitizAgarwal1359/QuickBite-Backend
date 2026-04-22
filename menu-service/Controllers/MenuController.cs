using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuickBite.Menu.DTOs;
using QuickBite.Menu.Interfaces;

namespace QuickBite.Menu.Controllers
{
    [ApiController]
    [Route("api/v1/menu")]
    [Produces("application/json")]
    public class MenuController : ControllerBase
    {
        private readonly IMenuService _menuService;
        private readonly ILogger<MenuController> _logger;

        public MenuController(IMenuService menuService, ILogger<MenuController> logger)
        {
            _menuService = menuService;
            _logger = logger;
        }

        // ─── Category Endpoints ──────────────────────────────────────────────────

        /// <summary>
        /// Add a new menu category (Owner only).
        /// </summary>
        [HttpPost("category")]
        [Authorize(Roles = "OWNER")]
        [ProducesResponseType(typeof(CategoryResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddCategory([FromBody] AddCategoryRequestDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _menuService.AddCategoryAsync(request);
            return StatusCode(StatusCodes.Status201Created, result);
        }

        /// <summary>
        /// Update a menu category (Owner only).
        /// </summary>
        [HttpPut("category/{id}")]
        [Authorize(Roles = "OWNER")]
        [ProducesResponseType(typeof(CategoryResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryRequestDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _menuService.UpdateCategoryAsync(id, request);
            return Ok(result);
        }

        /// <summary>
        /// Delete a menu category and all its items (Owner/Admin).
        /// </summary>
        [HttpDelete("category/{id}")]
        [Authorize(Roles = "OWNER,ADMIN")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            await _menuService.DeleteCategoryAsync(id);
            return Ok(new { message = "Category and its items deleted successfully" });
        }

        /// <summary>
        /// Get all categories for a restaurant.
        /// </summary>
        [HttpGet("categories/{restId}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<CategoryResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCategories(int restId)
        {
            var result = await _menuService.GetCategoriesByRestaurantAsync(restId);
            return Ok(result);
        }

        // ─── Full Menu ───────────────────────────────────────────────────────────

        /// <summary>
        /// Get the full menu for a restaurant (categories with nested available items).
        /// </summary>
        [HttpGet("restaurant/{id}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<CategoryResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetFullMenu(int id)
        {
            var result = await _menuService.GetMenuByRestaurantAsync(id);
            return Ok(result);
        }

        // ─── Item Endpoints ──────────────────────────────────────────────────────

        /// <summary>
        /// Add a new menu item to a category (Owner only).
        /// </summary>
        [HttpPost("item")]
        [Authorize(Roles = "OWNER")]
        [ProducesResponseType(typeof(MenuItemResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddMenuItem([FromBody] AddMenuItemRequestDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _menuService.AddMenuItemAsync(request);
            return StatusCode(StatusCodes.Status201Created, result);
        }

        /// <summary>
        /// Get a single menu item by ID.
        /// </summary>
        [HttpGet("item/{id}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(MenuItemResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetItem(int id)
        {
            var result = await _menuService.GetItemByIdAsync(id);
            return Ok(result);
        }

        /// <summary>
        /// Update a menu item (Owner only).
        /// </summary>
        [HttpPut("item/{id}")]
        [Authorize(Roles = "OWNER")]
        [ProducesResponseType(typeof(MenuItemResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateMenuItem(int id, [FromBody] UpdateMenuItemRequestDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _menuService.UpdateMenuItemAsync(id, request);
            return Ok(result);
        }

        /// <summary>
        /// Toggle menu item availability — in-stock/out-of-stock (Owner only).
        /// </summary>
        [HttpPut("item/{id}/toggle")]
        [Authorize(Roles = "OWNER")]
        [ProducesResponseType(typeof(MenuItemResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ToggleAvailability(int id)
        {
            var result = await _menuService.ToggleAvailabilityAsync(id);
            return Ok(result);
        }

        /// <summary>
        /// Delete a menu item (Owner/Admin).
        /// </summary>
        [HttpDelete("item/{id}")]
        [Authorize(Roles = "OWNER,ADMIN")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteMenuItem(int id)
        {
            await _menuService.DeleteMenuItemAsync(id);
            return Ok(new { message = "Menu item deleted successfully" });
        }

        // ─── Search & Filter ─────────────────────────────────────────────────────

        /// <summary>
        /// Search available menu items by keyword.
        /// </summary>
        [HttpGet("search")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<MenuItemResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Search([FromQuery] string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return BadRequest(new { message = "Search keyword is required" });
            var result = await _menuService.SearchMenuItemsAsync(keyword);
            return Ok(result);
        }

        /// <summary>
        /// Get all available veg items for a restaurant.
        /// </summary>
        [HttpGet("veg/{restId}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<MenuItemResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetVegItems(int restId)
        {
            var result = await _menuService.GetVegItemsAsync(restId);
            return Ok(result);
        }
    }
}
