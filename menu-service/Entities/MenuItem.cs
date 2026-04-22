using System.Text.Json.Serialization;

namespace QuickBite.Menu.Entities
{
    public class MenuItem
    {
        public int ItemId { get; set; }

        public int RestaurantId { get; set; }

        public int CategoryId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public double Price { get; set; }

        public double DiscountedPrice { get; set; }

        public string? ImageUrl { get; set; }

        public bool IsVeg { get; set; }

        public bool IsAvailable { get; set; } = true;

        public double Rating { get; set; } = 0;

        public int Calories { get; set; }

        public string? Tags { get; set; }

        // Navigation property
        [JsonIgnore]
        public MenuCategory? Category { get; set; }
    }
}
