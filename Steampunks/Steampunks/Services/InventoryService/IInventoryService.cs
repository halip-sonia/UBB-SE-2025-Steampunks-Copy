// <copyright file="IInventoryService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Steampunks.Services.InventoryService.InventoryService
{
    using System.Collections.Generic;
    using Steampunks.Domain.Entities;

    /// <summary>
    /// Interface for service that handles inventory-related operations.
    /// </summary>
    public interface IInventoryService
    {
        /// <summary>
        /// Retrieves a list of items from the user's inventory from the given game.
        /// </summary>
        /// <param name="game"> Game for which the items are retrieved. </param>
        /// <returns> A list of items from the game, from the user's inventory. </returns>
        List<Item> GetItemsFromInventory(Game game);

        /// <summary>
        /// Retrieves all the items of the user from the inventory.
        /// </summary>
        /// <returns> A list of all items from the inventory. </returns>
        List<Item> GetAllItemsFromInventory();

        /// <summary>
        /// Adds an item from a game to the inventory.
        /// </summary>
        /// <param name="game"> Game from which the item comes from. </param>
        /// <param name="item"> Item to be added to the inventory. </param>
        void AddItemToInventory(Game game, Item item);

        /// <summary>
        /// Removes an item from a game from the inventory.
        /// </summary>
        /// <param name="game"> Game from which the item comes from. </param>
        /// <param name="item"> Item to be removed from the inventory. </param>
        void RemoveItemFromInventory(Game game, Item item);
    }
}