using System;

namespace Steampunks.Domain.Entities
{
    public class Review
    {
        public int ReviewId { get; private set; }
        public User Reviewer { get; private set; }
        public Game Game { get; private set; }
        public string Content { get; private set; }
        public int Rating { get; private set; }
        public DateTime ReviewDate { get; private set; }

        private Review() { } // For EF Core

        public Review(User reviewer, Game game, string content, int rating)
        {
            Reviewer = reviewer;
            Game = game;
            Content = content;
            Rating = rating;
            ReviewDate = DateTime.UtcNow;
        }

        public void UpdateContent(string content)
        {
            Content = content;
        }

        public void UpdateRating(int rating)
        {
            if (rating < 1 || rating > 5)
                throw new ArgumentException("Rating must be between 1 and 5");
            
            Rating = rating;
        }
    }
} 