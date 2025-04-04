using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Steampunks.Domain.Entities;
using Steampunks.DataLink;

namespace Steampunks.Repository.Marketplace
{
    public class MarketplaceRepository : IMarketplaceRepository
    {
        private readonly DatabaseConnector _databaseConnector;

        public MarketplaceRepository()
        {
            _databaseConnector = new DatabaseConnector();
        }

        /// <summary>
        /// Retrieves all listed items available for sale from the database.
        /// </summary>
        /// <returns>
        /// A list of Item objects that are currently listed for sale.
        /// </returns>
        public List<Item> GetAllListedItems()
        {
            return _databaseConnector.GetAllListings();
        }

        /// <summary>
        /// Retrieves all currently listed items associated with a specific game.
        /// </summary>
        /// <param name="game">The Game entity for which to retrieve listed items.</param>
        /// <returns>
        /// A list of Item objects linked to the specified game and currently marked as listed.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the provided <paramref name="game"/> is null.
        /// </exception>
        public List<Item> GetListedItemsByGame(Game game)
        {
            if (game == null)
                throw new ArgumentNullException(nameof(game));

            return _databaseConnector.GetListingsByGame(game);
        }

        /// <summary>
        /// Marks an existing item as listed for the specified game.
        /// </summary>
        /// <param name="game">The Game associated with the item to be listed.</param>
        /// <param name="item">The Item to be marked as listed and updated with a new price.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="game"/> or <paramref name="item"/> is null.
        /// </exception>
        public void MakeItemListable(Game game, Item item)
        {
            if (game == null)
                throw new ArgumentNullException(nameof(game));
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            _databaseConnector.AddListing(item);
        }

        /// <summary>
        /// Marks an item as not listable in the specified game by updating its listing status.
        /// </summary>
        /// <param name="game">The game in which the item exists.</param>
        /// <param name="item">The item to be marked as not listable.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="game"/> or <paramref name="item"/> is null.
        /// </exception>
        public void MakeItemNotListable(Game game, Item item)
        {
            if (game == null)
                throw new ArgumentNullException(nameof(game));
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            _databaseConnector.RemoveListing(item);
        }

        /// <summary>
        /// Updates the price of an item in the specified game.
        /// </summary>
        /// <param name="game">The game in which the item exists.</param>
        /// <param name="item">The item whose price are being updated.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="game"/> or <paramref name="item"/> is null.
        /// </exception>
        public void UpdateItemPrice(Game game, Item item)
        {
            if (game == null)
                throw new ArgumentNullException(nameof(game));
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            _databaseConnector.UpdateListing(item);
        }
    }

    public interface IMarketplaceRepository
    {
        List<Item> GetAllListedItems();
        List<Item> GetListedItemsByGame(Game game);
        void MakeItemListable(Game game, Item item);
        void MakeItemNotListable(Game game, Item item);
        void UpdateItemPrice(Game game, Item item);
    }
}