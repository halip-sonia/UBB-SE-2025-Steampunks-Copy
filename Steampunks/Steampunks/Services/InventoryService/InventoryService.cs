// <copyright file="InventoryService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Steampunks.Services.InventoryService.InventoryService
{
    using System;
    using System.Collections.Generic;
    using Steampunks.Domain.Entities;
    using Steampunks.Repository.Inventory;

    /// <summary>
    /// Service that handles inventory-related operations.
    /// </summary>
    public class InventoryService : IInventoryService
    {
        private readonly IInventoryRepository inventoryRepository;
        private readonly User user;

        /// <summary>
        /// Initializes a new instance of the <see cref="InventoryService"/> class.
        /// </summary>
        /// <param name="inventoryRepository"> Inventory repository. </param>
        /// <param name="user"> User for which the inventory is managed. </param>
        /// <exception cref="ArgumentNullException"> Thrown if either the repository or the user is null.</exception>
        public InventoryService(InventoryRepository inventoryRepository, User user)
        {
            if (inventoryRepository != null)
            {
                this.inventoryRepository = inventoryRepository;
            }
            else
            {
                throw new ArgumentNullException(nameof(inventoryRepository));
            }

            if (user != null)
            {
                this.user = user;
            }
            else
            {
                throw new ArgumentNullException(nameof(user));
            }
        }

        /// <summary>
        /// Retrieves a list of items from the user's inventory from the given game.
        /// </summary>
        /// <param name="game"> Game for which the items are retrieved. </param>
        /// <returns> A list of items from the game, from the user's inventory. </returns>
        /// <exception cref="ArgumentNullException"> Throws an exception when the game is null. </exception>
        public List<Item> GetItemsFromInventory(Game game)
        {
            if (game == null)
            {
                throw new ArgumentNullException(nameof(game));
            }

            return this.inventoryRepository.GetItemsFromInventory(game);
        }

        /// <summary>
        /// Retrieves all the items of the user from the inventory.
        /// </summary>
        /// <returns> A list of all items from the inventory. </returns>
        public List<Item> GetAllItemsFromInventory()
        {
            return this.inventoryRepository.GetAllItemsFromInventory(this.user);
        }

        /// <summary>
        /// Adds an item from a game to the inventory.
        /// </summary>
        /// <param name="game"> Game from which the item comes from. </param>
        /// <param name="item"> Item to be added to the inventory. </param>
        /// <exception cref="ArgumentNullException"> Throws an exception if either the game or item is null. </exception>
        public void AddItemToInventory(Game game, Item item)
        {
            if (game == null)
            {
                throw new ArgumentNullException(nameof(game));
            }

            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            this.inventoryRepository.AddItemToInventory(game, item, this.user);
        }

        /// <summary>
        /// Removes an item from a game from the inventory.
        /// </summary>
        /// <param name="game"> Game from which the item comes from. </param>
        /// <param name="item"> Item to be removed from the inventory. </param>
        /// <exception cref="ArgumentNullException"> Throws an exception if either the game or item is null. </exception>
        public void RemoveItemFromInventory(Game game, Item item)
        {
            if (game == null)
            {
                throw new ArgumentNullException(nameof(game));
            }

            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            this.inventoryRepository.RemoveItemFromInventory(game, item, this.user);
        }
    }
} 