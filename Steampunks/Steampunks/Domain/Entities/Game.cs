using System.Collections.Generic;

namespace Steampunks.Domain.Entities
{
    public class Game
    {
        public int GameId { get; private set; }
        public string Title { get; private set; }
        public float Price { get; private set; }
        public string Genre { get; private set; }
        public string Description { get; private set; }
        public ICollection<Review> GameReviews { get; private set; }
        public GameStatus Status { get; private set; }
        public float RecommendedSpecs { get; private set; }
        public float MinimumSpecs { get; private set; }

        private Game() { } // For EF Core

        public Game(string title, float price, string genre, string description)
        {
            Title = title;
            Price = price;
            Genre = genre;
            Description = description;
            GameReviews = new List<Review>();
        }

        public void SetTitle(string title)
        {
            Title = title;
        }

        public void SetPrice(float price)
        {
            Price = price;
        }

        public void SetGenre(string genre)
        {
            Genre = genre;
        }

        public void SetGameId(int id)
        {
            GameId = id;
        }

        public void SetName(string name)
        {
            Title = name;
        }

        public void SetDescription(string description)
        {
            Description = description;
        }

        public void AddReview(Review review)
        {
            GameReviews.Add(review);
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