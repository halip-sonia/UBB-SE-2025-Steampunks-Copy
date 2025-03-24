using System.Collections.Generic;

namespace Steampunks.Domain.Entities
{
    public class Game
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int GameId { get; private set; }
        public string Title { get; private set; }
        public float Price { get; private set; }
        public string Genre { get; private set; }
        public string Description { get; private set; }
        public ICollection<Review> GameReviews { get; private set; }
        public string Status { get; private set; }
        public float RecommendedSpecs { get; private set; }
        public float MinimumSpecs { get; private set; }

        private Game() 
        { 
            Title = string.Empty;
            Genre = string.Empty;
            Description = string.Empty;
            GameReviews = new List<Review>();
        } // For EF Core

        public Game(string title, float price, string genre, string description)
        {
            Title = title;
            Price = price;
            Genre = genre;
            Description = description;
            GameReviews = new List<Review>();
            Status = "Available";
        }

        public void SetGameId(int id)
        {
            GameId = id;
        }

        public string GetTitle()
        {
            return Title;
        }

        public void SetTitle(string title)
        {
            Title = title;
        }

        public float GetPrice()
        {
            return Price;
        }

        public void SetPrice(float price)
        {
            Price = price;
        }

        public string GetGenre()
        {
            return Genre;
        }

        public void SetGenre(string genre)
        {
            Genre = genre;
        }

        public string GetDescription()
        {
            return Description;
        }

        public void SetDescription(string description)
        {
            Description = description;
        }

        public string GetStatus()
        {
            return Status;
        }

        public void SetStatus(string status)
        {
            Status = status;
        }

        public void AddReview(Review review)
        {
            GameReviews.Add(review);
        }

        public override string ToString()
        {
            return Price > 0 ? $"{Title} (${Price:F2})" : Title;
        }
    }

    public enum GameStatus
    {
        Available,
        Unavailable,
        InDevelopment,
        Discontinued
    }
} 