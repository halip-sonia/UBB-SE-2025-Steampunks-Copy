using System;
using System.Collections.Generic;

namespace Steampunks.Domain.Entities
{
    public class ItemTrade
    {
        public int TradeId { get; private set; }
        public User SourceUser { get; private set; }
        public User DestinationUser { get; private set; }
        public Game GameOfTrade { get; private set; }
        public ICollection<Item> SourceUserTradeItems { get; private set; }
        public ICollection<Item> DestinationUserTradeItems { get; private set; }
        public DateTime TradeDate { get; private set; }
        public string TradeDescription { get; private set; }
        public string TradeStatus { get; private set; }
        public bool AcceptedBySourceUser { get; private set; }
        public bool AcceptedByDestinationUser { get; private set; }

        private ItemTrade() { } // For EF Core

        public ItemTrade(User sourceUser, User destinationUser, Game game, string description)
        {
            SourceUser = sourceUser;
            DestinationUser = destinationUser;
            GameOfTrade = game;
            TradeDescription = description;
            TradeDate = DateTime.UtcNow;
            TradeStatus = "Pending";
            AcceptedBySourceUser = false;
            AcceptedByDestinationUser = false;
            SourceUserTradeItems = new List<Item>();
            DestinationUserTradeItems = new List<Item>();
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

        public void SetTradeItems(ICollection<Item> sourceItems, ICollection<Item> destinationItems)
        {
            SourceUserTradeItems = sourceItems;
            DestinationUserTradeItems = destinationItems;
        }

        public void SetTradeStatus(string status)
        {
            TradeStatus = status;
        }

        public void SetTradeDescription(string description)
        {
            TradeDescription = description;
        }
    }
} 