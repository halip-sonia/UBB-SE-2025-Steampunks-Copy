using System;

namespace Steampunks.Domain.Entities
{
    public class GameTrade
    {
        public int TradeId { get; private set; }
        public User SourceUser { get; private set; }
        public User DestinationUser { get; private set; }
        public DateTime TradeDate { get; private set; }
        public string TradeDescription { get; private set; }
        public bool AcceptedBySourceUser { get; private set; }
        public bool AcceptedByDestinationUser { get; private set; }
        public Game TradeGame { get; private set; }
        public string TradeStatus { get; private set; }

        private GameTrade() { } // For EF Core

        public GameTrade(User sourceUser, User destinationUser, Game game, string description)
        {
            SourceUser = sourceUser;
            DestinationUser = destinationUser;
            TradeGame = game;
            TradeDescription = description;
            TradeDate = DateTime.UtcNow;
            AcceptedBySourceUser = false;
            AcceptedByDestinationUser = false;
            TradeStatus = "Pending";
        }

        public void SetTradeId(int id)
        {
            TradeId = id;
        }

        public User GetSourceUser()
        {
            return SourceUser;
        }

        public void SetSourceUser(User user)
        {
            SourceUser = user;
        }

        public User GetDestinationUser()
        {
            return DestinationUser;
        }

        public void SetDestinationUser(User user)
        {
            DestinationUser = user;
        }

        public void SetTradeDescription(string description)
        {
            TradeDescription = description;
        }

        public void SetTradeStatus(string status)
        {
            TradeStatus = status;
        }

        public Game GetTradeGame()
        {
            return TradeGame;
        }
    }
} 