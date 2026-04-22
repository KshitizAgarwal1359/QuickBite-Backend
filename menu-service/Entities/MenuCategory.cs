using System.Text.Json.Serialization;

namespace QuickBite.Menu.Entities
{
    public class MenuCategory
    {
        public int CategoryId { get; set; }

        public int RestaurantId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }

        public int DisplayOrder { get; set; }

        // Navigation property
        [JsonIgnore]
        public List<MenuItem> Items { get; set; } = new();
    }
}
