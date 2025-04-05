﻿// <copyright file="IMarketplaceService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Steampunks.Services.MarketplaceService
{
    using System.Collections.Generic;
    using Steampunks.Domain.Entities;

    /// <summary>
    /// Interface for marketplace-related operations.
    /// </summary>
    public interface IMarketplaceService
    {
        /// <summary>
        /// Adds a listing for an item from a game to the marketplace.
        /// </summary>
        /// <param name="game"> Game from which the item being added is. </param>
        /// <param name="item"> Item for which the listing is added. </param>
        void AddListing(Game game, Item item);

        /// <summary>
        /// Handles the purchase of an item by removing former ownership, adding the item to the new owner's inventory,
        /// and marking the item as unlisted.
        /// </summary>
        /// <param name="item"> Item to be purchased. </param>
        /// <returns> True upon successful purchase. </returns>
        bool BuyItem(Item item);

        /// <summary>
        /// Retrieves all the listings in the marketplace.
        /// </summary>
        /// <returns> A list of Items (marketplace listings). </returns>
        List<Item> GetAllListings();

        /// <summary>
        /// Retrieve all users from the database.
        /// </summary>
        /// <returns> A list of all Users. </returns>
        List<User> GetAllUsers();

        /// <summary>
        /// Returns the current user.
        /// </summary>
        /// <returns> The current user. </returns>
        User GetCurrentUser();

        /// <summary>
        /// Retrieves listings from a certain game.
        /// </summary>
        /// <param name="game"> Game for which the listing is retrieved. </param>
        /// <returns> A list of Items (marketplace listings). </returns>
        List<Item> GetListingsByGame(Game game);

        /// <summary>
        /// Remove listing (an item from a game).
        /// </summary>
        /// <param name="game"> Game from which the item to be listed is. </param>
        /// <param name="item"> Item listing to be removed. </param>
        void RemoveListing(Game game, Item item);

        /// <summary>
        /// Sets the current user to another.
        /// </summary>
        /// <param name="user"> User to be set. </param>
        void SetCurrentUser(User user);

        /// <summary>
        /// Update listing (an item from a game).
        /// </summary>
        /// <param name="game"> Game from which the item to be listed is. </param>
        /// <param name="item"> Item listing to be updated. </param>
        void UpdateListing(Game game, Item item);
    }
}