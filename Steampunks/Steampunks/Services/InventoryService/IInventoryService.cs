// <copyright file="IInventoryService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Steampunks.Services.InventoryService.InventoryService
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
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
        Task<List<Item>> GetItemsFromInventoryAsync(Game game);

        /// <summary>
        /// Retrieves all the items of the user from the inventory.
        /// </summary>
        /// <param name="user">The user whose inventory is to be retrieved.</param>
        /// <returns> A list of all items from the inventory. </returns>
        Task<List<Item>> GetAllItemsFromInventoryAsync(User user);

        /// <summary>
        /// Adds an item from a game to the inventory.
        /// </summary>
        /// <param name="game"> Game from which the item comes from. </param>
        /// <param name="item"> Item to be added to the inventory. </param>
        /// <param name="user">The user who is adding the item to their inventory.</param>
        /// <returns>AddInventoryItemAsync returns <see cref="Task"/> representing the asynchronous operation.</returns>
        Task AddItemToInventoryAsync(Game game, Item item, User user);

        /// <summary>
        /// Removes an item from a game from the inventory.
        /// </summary>
        /// <param name="game"> Game from which the item comes from. </param>
        /// <param name="item"> Item to be removed from the inventory. </param>
        /// <param name="user">The user whose inventory the item is being removed from.</param>
        /// <returns>RemoveInventoryItemAsync returns <see cref="Task"/> representing the asynchronous operation.</returns>
        Task RemoveItemFromInventoryAsync(Game game, Item item, User user);
    }
}