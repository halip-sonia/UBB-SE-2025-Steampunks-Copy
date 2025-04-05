namespace Steampunks.Repository.Inventory
{
    using System;
    using System.Collections.Generic;
    using Steampunks.DataLink;
    using Steampunks.Domain.Entities;

    /// <summary>
    /// Provides functionality to manage inventory items associated with users and games.
    /// </summary>
    public class InventoryRepository : IInventoryRepository
    {
        /// <summary>
        /// The database connector used for inventory-related data operations.
        /// </summary>
        private readonly DatabaseConnector dataBaseConnector;

        /// <summary>
        /// Initializes a new instance of the <see cref="InventoryRepository"/> class.
        /// </summary>
        public InventoryRepository()
        {
            this.dataBaseConnector = new DatabaseConnector();
        }

        /// <summary>
        /// Retrieves a list of items from a specific game's inventory.
        /// </summary>
        /// <param name="game">The game whose inventory items are to be retrieved.</param>
        /// <returns>A list of <see cref="Item"/> objects associated with the specified game.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="game"/> is null.</exception>
        public List<Item> GetItemsFromInventory(Game game)
        {
            if (game == null)
            {
                throw new ArgumentNullException(nameof(game));
            }

            return this.dataBaseConnector.GetInventoryItems(game);
        }

        /// <summary>
        /// Retrieves all inventory items associated with a specific user across all games.
        /// </summary>
        /// <param name="user">The user whose inventory is to be retrieved.</param>
        /// <returns>A list of all <see cref="Item"/> objects belonging to the specified user.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="user"/> is null.</exception>
        public List<Item> GetAllItemsFromInventory(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return this.dataBaseConnector.GetAllInventoryItems(user);
        }

        /// <summary>
        /// Adds an item to a specific game's inventory for a user.
        /// </summary>
        /// <param name="game">The game to which the item is to be added.</param>
        /// <param name="item">The item to be added.</param>
        /// <param name="user">The user who is adding the item to their inventory.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="game"/>, <paramref name="item"/>, or <paramref name="user"/> is null.
        /// </exception>
        public void AddItemToInventory(Game game, Item item, User user)
        {
            if (game == null)
            {
                throw new ArgumentNullException(nameof(game));
            }

            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            this.dataBaseConnector.AddInventoryItem(game, item, user);
        }

        /// <summary>
        /// Removes an item from a specific game's inventory for a user.
        /// </summary>
        /// <param name="game">The game from which the item is to be removed.</param>
        /// <param name="item">The item to be removed.</param>
        /// <param name="user">The user whose inventory the item is being removed from.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="game"/>, <paramref name="item"/>, or <paramref name="user"/> is null.
        /// </exception>
        public void RemoveItemFromInventory(Game game, Item item, User user)
        {
            if (game == null)
            {
                throw new ArgumentNullException(nameof(game));
            }

            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            this.dataBaseConnector.RemoveInventoryItem(game, item, user);
        }
    }
}