// <copyright file="InventoryService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Steampunks.Services.InventoryService.InventoryService
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Steampunks.Domain.Entities;
    using Steampunks.Repository.Inventory;
    using Steampunks.Validators.InventoryValidator.InventoryValidator;

    /// <summary>
    /// Service that handles inventory-related operations.
    /// </summary>
    public class InventoryService : IInventoryService
    {
        private readonly IInventoryRepository inventoryRepository;
        private readonly IInventoryValidator inventoryValidator;

        /// <summary>
        /// Initializes a new instance of the <see cref="InventoryService"/> class.
        /// </summary>
        /// <param name="inventoryRepository">The inventory repository.</param>
        /// <param name="inventoryValidator">The inventory validator.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if either the repository or the validator is null.
        /// </exception>
        public InventoryService(IInventoryRepository inventoryRepository, IInventoryValidator inventoryValidator)
        {
            this.inventoryRepository = inventoryRepository ?? throw new ArgumentNullException(nameof(inventoryRepository));
            this.inventoryValidator = inventoryValidator ?? throw new ArgumentNullException(nameof(inventoryValidator));
        }

        /// <summary>
        /// Retrieves a list of items from the user's inventory for the specified game.
        /// </summary>
        /// <param name="game">The game for which items are retrieved.</param>
        /// <returns>
        /// A task representing the asynchronous operation. The task result contains a list of items.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="game"/> is null.
        /// </exception>
        public async Task<List<Item>> GetItemsFromInventoryAsync(Game game)
        {
            this.inventoryValidator.ValidateGame(game);
            return await this.inventoryRepository.GetItemsFromInventoryAsync(game);
        }

        /// <summary>
        /// Retrieves all items from the user's inventory.
        /// </summary>
        /// <param name="user">The user whose inventory is to be retrieved.</param>
        /// <returns>
        /// A task representing the asynchronous operation. The task result contains a list of all items.
        /// </returns>
        public async Task<List<Item>> GetAllItemsFromInventoryAsync(User user)
        {
            this.inventoryValidator.ValidateUser(user);
            return await this.inventoryRepository.GetAllItemsFromInventoryAsync(user);
        }

        /// <summary>
        /// Adds an item to the inventory for the specified game and user.
        /// </summary>
        /// <param name="game">The game from which the item comes.</param>
        /// <param name="item">The item to be added.</param>
        /// <param name="user">The user adding the item.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="game"/>, <paramref name="item"/>, or <paramref name="user"/> is null.
        /// </exception>
        public async Task AddItemToInventoryAsync(Game game, Item item, User user)
        {
            this.inventoryValidator.ValidateInventoryOperation(game, item, user);
            await this.inventoryRepository.AddItemToInventoryAsync(game, item, user);
        }

        /// <summary>
        /// Removes an item from the inventory for the specified game and user.
        /// </summary>
        /// <param name="game">The game from which the item comes.</param>
        /// <param name="item">The item to be removed.</param>
        /// <param name="user">The user whose inventory the item is being removed from.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="game"/>, <paramref name="item"/>, or <paramref name="user"/> is null.
        /// </exception>
        public async Task RemoveItemFromInventoryAsync(Game game, Item item, User user)
        {
            this.inventoryValidator.ValidateInventoryOperation(game, item, user);
            await this.inventoryRepository.RemoveItemFromInventoryAsync(game, item, user);
        }
    }
}
