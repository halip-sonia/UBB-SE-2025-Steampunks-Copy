using System;
using System.Collections.Generic;

namespace Steampunks.Models
{
    public class Game
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Genre { get; set; }
        public string DeveloperName { get; set; }
        public DateTime ReleaseDate { get; set; }
        public float Rating { get; set; }
        public string ImageUrl { get; set; }
        public int ReviewCount { get; set; }
    }
} 