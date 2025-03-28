using System;
using System.Collections.Generic;

namespace Steampunks.Domain.Entities
{
    public class ItemTrade
    {
        private int _tradeId;
        private User _sourceUser;
        private User _destinationUser;
        private Game _gameOfTrade;
        private DateTime _tradeDate;
        private string _tradeDescription;
        private string _tradeStatus;
        private bool _acceptedBySourceUser;
        private bool _acceptedByDestinationUser;
        private List<Item> _sourceUserItems;
        private List<Item> _destinationUserItems;

        public int TradeId => _tradeId;
        public User SourceUser => _sourceUser;
        public User DestinationUser => _destinationUser;
        public Game GameOfTrade => _gameOfTrade;
        public DateTime TradeDate => _tradeDate;
        public string TradeDescription => _tradeDescription;
        public string TradeStatus => _tradeStatus;
        public bool AcceptedBySourceUser => _acceptedBySourceUser;
        public bool AcceptedByDestinationUser => _acceptedByDestinationUser;
        public IReadOnlyList<Item> SourceUserItems => _sourceUserItems;
        public IReadOnlyList<Item> DestinationUserItems => _destinationUserItems;

        public ItemTrade(User sourceUser, User destinationUser, Game gameOfTrade, string description)
        {
            _sourceUser = sourceUser ?? throw new ArgumentNullException(nameof(sourceUser));
            _destinationUser = destinationUser ?? throw new ArgumentNullException(nameof(destinationUser));
            _gameOfTrade = gameOfTrade ?? throw new ArgumentNullException(nameof(gameOfTrade));
            _tradeDescription = description ?? throw new ArgumentNullException(nameof(description));
            _tradeDate = DateTime.UtcNow;
            _tradeStatus = "Pending";
            _acceptedBySourceUser = false;
            _acceptedByDestinationUser = false;
            _sourceUserItems = new List<Item>();
            _destinationUserItems = new List<Item>();
        }

        public void SetTradeId(int tradeId)
        {
            _tradeId = tradeId;
        }

        public void AddSourceUserItem(Item item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            _sourceUserItems.Add(item);
        }

        public void AddDestinationUserItem(Item item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            _destinationUserItems.Add(item);
        }

        public void AcceptBySourceUser()
        {
            _acceptedBySourceUser = true;
            if (_acceptedByDestinationUser)
            {
                _tradeStatus = "Completed";
            }
        }

        public void AcceptByDestinationUser()
        {
            _acceptedByDestinationUser = true;
            if (_acceptedBySourceUser)
            {
                _tradeStatus = "Completed";
            }
        }

        public void Decline()
        {
            _tradeStatus = "Declined";
            _acceptedBySourceUser = false;
            _acceptedByDestinationUser = false;
        }

        public void Complete()
        {
            _tradeStatus = "Completed";
            _acceptedBySourceUser = true;
            _acceptedByDestinationUser = true;
        }
    }
} 