using System;

namespace Steampunks.Models
{
    public class Skin
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Game { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Rarity { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public DateTime ReleaseDate { get; set; }
    }
} 