namespace Steampunks.Repository.Marketplace
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Steampunks.Domain.Entities;

    /// <summary>
    /// Provides repository methods for interacting with marketplace items.
    /// Allows listing, unlisting, updating, and retrieving item listings associated with games.
    /// </summary>
    public interface IMarketplaceRepository
    {
        /// <summary>
        /// Retrieves all listed items available for sale from the database.
        /// </summary>
        /// <returns>
        /// A list of Item objects that are currently listed for sale.
        /// </returns>
        List<Item> GetAllListedItems();

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
        List<Item> GetListedItemsByGame(Game game);

        /// <summary>
        /// Marks an existing item as listed for the specified game.
        /// </summary>
        /// <param name="game">The Game associated with the item to be listed.</param>
        /// <param name="item">The Item to be marked as listed and updated with a new price.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="game"/> or <paramref name="item"/> is null.
        /// </exception>
        void MakeItemListable(Game game, Item item);

        /// <summary>
        /// Marks an item as not listable in the specified game by updating its listing status.
        /// </summary>
        /// <param name="game">The game in which the item exists.</param>
        /// <param name="item">The item to be marked as not listable.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="game"/> or <paramref name="item"/> is null.
        /// </exception>
        void MakeItemNotListable(Game game, Item item);

        /// <summary>
        /// Updates the price of an item in the specified game.
        /// </summary>
        /// <param name="game">The game in which the item exists.</param>
        /// <param name="item">The item whose price are being updated.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="game"/> or <paramref name="item"/> is null.
        /// </exception>
        void UpdateItemPrice(Game game, Item item);
    }
}
