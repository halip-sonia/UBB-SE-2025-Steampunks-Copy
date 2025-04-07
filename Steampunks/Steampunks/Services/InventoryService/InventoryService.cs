// <copyright file="InventoryService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Steampunks.Services.InventoryService.InventoryService
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Steampunks.Domain.Entities;
    using Steampunks.Repository.Inventory;
    using Steampunks.Validators.InventoryValidator.InventoryValidator;
    using Steampunks.Validators.InventoryValidators;

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
        /// <exception cref="ArgumentNullException">
        /// Thrown if the repository is null.
        /// </exception>
        public InventoryService(IInventoryRepository inventoryRepository)
        {
            this.inventoryRepository = inventoryRepository ?? throw new ArgumentNullException(nameof(inventoryRepository));
            // Instantiate the validator with enriched logic.
            this.inventoryValidator = new InventoryValidator();
        }

        /// <summary>
        /// Retrieves a list of items from the user's inventory for the specified game.
        /// </summary>
        public async Task<List<Item>> GetItemsFromInventoryAsync(Game game)
        {
            // Validate the game.
            this.inventoryValidator.ValidateGame(game);
            return await this.inventoryRepository.GetItemsFromInventoryAsync(game);
        }

        /// <summary>
        /// Retrieves all items from the user's inventory.
        /// </summary>
        public async Task<List<Item>> GetAllItemsFromInventoryAsync(User user)
        {
            // Validate the user.
            this.inventoryValidator.ValidateUser(user);
            return await this.inventoryRepository.GetAllItemsFromInventoryAsync(user);
        }

        /// <summary>
        /// Adds an item to the inventory for the specified game and user.
        /// </summary>
        public async Task AddItemToInventoryAsync(Game game, Item item, User user)
        {
            // Validate the inventory operation.
            this.inventoryValidator.ValidateInventoryOperation(game, item, user);
            await this.inventoryRepository.AddItemToInventoryAsync(game, item, user);
        }

        /// <summary>
        /// Removes an item from the inventory for the specified game and user.
        /// </summary>
        public async Task RemoveItemFromInventoryAsync(Game game, Item item, User user)
        {
            // Validate the inventory operation.
            this.inventoryValidator.ValidateInventoryOperation(game, item, user);
            await this.inventoryRepository.RemoveItemFromInventoryAsync(game, item, user);
        }

        /// <summary>
        /// Gets the inventory of a given user by their user ID asynchronously.
        /// </summary>
        public async Task<List<Item>> GetUserInventoryAsync(int userId)
        {
            if (userId <= 0)
            {
                throw new ArgumentException("UserId must be positive.", nameof(userId));
            }
            return await this.inventoryRepository.GetUserInventoryAsync(userId);
        }

        /// <summary>
        /// Retrieves all users asynchronously.
        /// </summary>
        public async Task<List<User>> GetAllUsersAsync()
        {
            return await this.inventoryRepository.GetAllUsersAsync();
        }

        /// <summary>
        /// Sells an item asynchronously.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<bool> SellItemAsync(Item item)
        {
            // Validate that the item is sellable.
            this.inventoryValidator.ValidateSellableItem(item);
            return await this.inventoryRepository.SellItemAsync(item);
        }

        /// <summary>
        /// Filters inventory items based on the selected game and search text.
        /// </summary>
        /// <param name="items">The list of inventory items to filter.</param>
        /// <param name="selectedGame">
        /// The game to filter by. Assumes the "All Games" option is identified by its title.
        /// </param>
        /// <param name="searchText">The text used to search item names and descriptions.</param>
        /// <returns>A filtered list of items.</returns>
        public List<Item> FilterInventoryItems(List<Item> items, Game selectedGame, string searchText)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            // Exclude items that are already listed.
            var filteredItems = items.Where(item => !item.IsListed);

            // If a specific game is selected (not the "All Games" option), filter by that game.
            if (selectedGame != null && selectedGame.Title != "All Games")
            {
                filteredItems = filteredItems.Where(item =>
                    item.Game != null && item.Game.GameId == selectedGame.GameId);
            }

            // Apply search text filter on item name or description (case-insensitive).
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                filteredItems = filteredItems.Where(item =>
                    (!string.IsNullOrEmpty(item.ItemName) &&
                     item.ItemName.Contains(searchText, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(item.Description) &&
                     item.Description.Contains(searchText, StringComparison.OrdinalIgnoreCase)));
            }

            return filteredItems.ToList();
        }

        /// <summary>
        /// Retrieves a list of available games based on the provided inventory items.
        /// Includes a special "All Games" option as the first entry.
        /// </summary>
        /// <param name="items">The list of inventory items.</param>
        /// <returns>A list of games.</returns>
        public List<Game> GetAvailableGames(List<Item> items)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            // Create the special "All Games" option.
            var allGamesOption = new Game("All Games", 0.0f, string.Empty, "Show items from all games");

            // Start with the "All Games" option.
            var availableGames = new List<Game> { allGamesOption };

            // Add unique games from the inventory.
            var uniqueGames = items
                .Select(item => item.Game)
                .Where(game => game != null)
                .Distinct(new GameComparer());

            availableGames.AddRange(uniqueGames);
            return availableGames;
        }

        /// <summary>
        /// Retrieves the filtered inventory for a given user based on selected game and search text.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="selectedGame">The game filter.</param>
        /// <param name="searchText">The search text for item name/description.</param>
        /// <returns>A task that represents the asynchronous operation containing the filtered list of items.</returns>
        public async Task<List<Item>> GetUserFilteredInventoryAsync(int userId, Game selectedGame, string searchText)
        {
            var allItems = await this.GetUserInventoryAsync(userId);
            return this.FilterInventoryItems(allItems, selectedGame, searchText);
        }

        /// <summary>
        /// Provides a custom comparer for <see cref="Game"/> objects based on the GameId.
        /// </summary>
        private class GameComparer : IEqualityComparer<Game>
        {
            public bool Equals(Game x, Game y)
            {
                if (x == null || y == null)
                {
                    return false;
                }

                return x.GameId == y.GameId;
            }

            public int GetHashCode(Game obj)
            {
                if (obj == null)
                {
                    return 0;
                }

                return obj.GameId.GetHashCode();
            }
        }
    }
}
