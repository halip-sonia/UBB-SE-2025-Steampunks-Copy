using System;
using System.Collections.Generic;

namespace Steampunks.Models
{
    public class Game
    {
        private int gameId;
        private string title;
        private float price;
        private string genre;
        private int developerId;
        public List<Review> game_reviews { get; set; }
        public GameStatus status { get; set; }
        public string RecommendedSpecs { get; set; }
        public string MinimumSpecs { get; set; }
        public string image_url { get; set; }

        public void setTitle(string title)
        {
            this.title = title;
        }

        public void setPrice(float price)
        {
            this.price = price;
        }

        public void setGenre(string genre)
        {
            this.genre = genre;
        }

        public void setGameId(int id)
        {
            this.gameId = id;
        }

        public string getTitle()
        {
            return title;
        }

        public float getPrice()
        {
            return price;
        }

        public string getGenre()
        {
            return genre;
        }

        public int getDeveloperId()
        {
            return developerId;
        }

        public void addReview(Review review)
        {
            if (game_reviews == null)
                game_reviews = new List<Review>();
            game_reviews.Add(review);
        }

        public List<Review> getAllReviews()
        {
            return game_reviews;
        }
    }

    public enum GameStatus
    {
        Available,
        Unavailable,
        Upcoming,
        Maintenance
    }
} 