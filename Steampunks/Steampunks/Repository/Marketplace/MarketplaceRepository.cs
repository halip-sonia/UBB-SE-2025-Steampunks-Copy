using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Steampunks.Domain.Entities;
using Steampunks.DataLink;

namespace Steampunks.Repository.Marketplace
{
    public class MarketplaceRepository : IMarketplaceRepository
    {
        private readonly DatabaseConnector _dbConnector;

        public MarketplaceRepository()
        {
            _dbConnector = new DatabaseConnector();
        }

        public List<Item> GetAllListings()
        {
            return _dbConnector.GetAllListings();
        }

        public List<Item> GetListingsByGame(Game game)
        {
            if (game == null)
                throw new ArgumentNullException(nameof(game));

            return _dbConnector.GetListingsByGame(game);
        }

        public void AddListing(Game game, Item item)
        {
            if (game == null)
                throw new ArgumentNullException(nameof(game));
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            _dbConnector.AddListing(item);
        }

        public void RemoveListing(Game game, Item item)
        {
            if (game == null)
                throw new ArgumentNullException(nameof(game));
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            _dbConnector.RemoveListing(item);
        }

        public void UpdateListing(Game game, Item item)
        {
            if (game == null)
                throw new ArgumentNullException(nameof(game));
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            _dbConnector.UpdateListing(item);
        }

        public bool BuyItem(Item item, User currentUser)
        {
            return _dbConnector.BuyItem(item, currentUser);
        }

    }

    public interface IMarketplaceRepository
    {
        List<Item> GetAllListings();
        List<Item> GetListingsByGame(Game game);
        void AddListing(Game game, Item item);
        void RemoveListing(Game game, Item item);
        void UpdateListing(Game game, Item item);
        bool BuyItem(Item item, User currentUser);

    }
}