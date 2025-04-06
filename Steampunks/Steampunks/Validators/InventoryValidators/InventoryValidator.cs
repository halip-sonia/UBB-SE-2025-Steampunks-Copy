// <copyright file="InventoryValidator.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Steampunks.Validators.InventoryValidators
{
    using System;
    using Steampunks.Domain.Entities;
    using Steampunks.Validators.InventoryValidator.InventoryValidator;

    /// <summary>
    /// Provides enriched validation logic for inventory operations.
    /// </summary>
    public class InventoryValidator : IInventoryValidator
    {
        /// <inheritdoc/>
        public void ValidateGame(Game game)
        {
            if (game == null)
            {
                throw new ArgumentNullException(nameof(game), "Game cannot be null.");
            }

            if (string.IsNullOrWhiteSpace(game.Title))
            {
                throw new ArgumentException("Game title cannot be null or empty.", nameof(game));
            }

            if (game.Price < 0)
            {
                throw new ArgumentException("Game price cannot be negative.", nameof(game));
            }
        }

        /// <inheritdoc/>
        public void ValidateItem(Item item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item), "Item cannot be null.");
            }

            if (string.IsNullOrWhiteSpace(item.ItemName))
            {
                throw new ArgumentException("Item name cannot be null or empty.", nameof(item));
            }
        }

        /// <inheritdoc/>
        public void ValidateUser(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user), "User cannot be null.");
            }

            if (string.IsNullOrWhiteSpace(user.Username))
            {
                throw new ArgumentException("User username cannot be null or empty.", nameof(user));
            }
        }

        /// <inheritdoc/>
        public void ValidateInventoryOperation(Game game, Item item, User user)
        {
            this.ValidateGame(game);
            this.ValidateItem(item);
            this.ValidateUser(user);
        }
    }
}
