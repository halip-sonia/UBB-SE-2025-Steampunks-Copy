// <copyright file="IInventoryRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Steampunks.Repository.Inventory
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
        Task<List<Item>> GetItemsFromInventoryAsync(Game game);

        /// <summary>
        /// Get the inventory of a given User by it's userID Asynchronously.
        /// </summary>
        /// <param name="userId">The id of the user whose inventory items are to be retrieved.</param>
        /// <returns>A <see cref="Task"/> asynchronously resolving to a list of <see cref="Item"/> objects associated with the specified user.</returns>
        Task<List<Item>> GetUserInventoryAsync(int userId);

        /// <summary>
        /// Retrieves all items across all games associated with a specific user.
        /// </summary>
        /// <param name="user">The user whose inventory items should be retrieved.</param>
        /// <returns>A list of <see cref="Item"/> objects belonging to the specified user.</returns>
        Task<List<Item>> GetAllItemsFromInventoryAsync(User user);

        /// <summary>
        /// Adds an item to a specific game's inventory for a user.
        /// </summary>
        /// <param name="game">The game to which the item will be added.</param>
        /// <param name="item">The item to be added to the inventory.</param>
        /// <param name="user">The user who owns the inventory being updated.</param>
        /// <returns>AddInventoryItemAsync returns <see cref="Task"/> representing the asynchronous operation.</returns>
        Task AddItemToInventoryAsync(Game game, Item item, User user);

        /// <summary>
        /// Removes an item from a specific game's inventory for a user.
        /// </summary>
        /// <param name="game">The game from which the item will be removed.</param>
        /// <param name="item">The item to be removed from the inventory.</param>
        /// <param name="user">The user who owns the inventory being updated.</param>
        /// <returns>RemoveInventoryItemAsync returns <see cref="Task"/> representing the asynchronous operation.</returns>
        Task RemoveItemFromInventoryAsync(Game game, Item item, User user);

        Task<List<User>> GetAllUsersAsync();

        Task<bool> SellItemAsync(Item item);
    }
}
