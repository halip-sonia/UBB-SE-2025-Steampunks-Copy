﻿namespace Steampunks.Repository.Inventory
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Steampunks.Domain.Entities;

    /// <summary>
    /// Defines methods for interacting with inventory data associated with users and games.
    /// </summary>
    public interface IInventoryRepository
    {
        /// <summary>
        /// Retrieves a list of items associated with a specific game's inventory.
        /// </summary>
        /// <param name="game">The game whose inventory items should be retrieved.</param>
        /// <returns>A list of <see cref="Item"/> objects related to the specified game.</returns>
        List<Item> GetItemsFromInventory(Game game);

        /// <summary>
        /// Retrieves all items across all games associated with a specific user.
        /// </summary>
        /// <param name="user">The user whose inventory items should be retrieved.</param>
        /// <returns>A list of <see cref="Item"/> objects belonging to the specified user.</returns>
        List<Item> GetAllItemsFromInventory(User user);

        /// <summary>
        /// Adds an item to a specific game's inventory for a user.
        /// </summary>
        /// <param name="game">The game to which the item will be added.</param>
        /// <param name="item">The item to be added to the inventory.</param>
        /// <param name="user">The user who owns the inventory being updated.</param>
        void AddItemToInventory(Game game, Item item, User user);

        /// <summary>
        /// Removes an item from a specific game's inventory for a user.
        /// </summary>
        /// <param name="game">The game from which the item will be removed.</param>
        /// <param name="item">The item to be removed from the inventory.</param>
        /// <param name="user">The user who owns the inventory being updated.</param>
        void RemoveItemFromInventory(Game game, Item item, User user);
    }
}
