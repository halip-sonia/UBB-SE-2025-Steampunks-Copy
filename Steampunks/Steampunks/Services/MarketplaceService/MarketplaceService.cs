namespace Steampunks.Services.MarketplaceService
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Steampunks.DataLink;
    using Steampunks.Domain.Entities;
    using Steampunks.Repository.Marketplace;

    /// <summary>
    /// Service that manages marketplace operations.
    /// </summary>
    public class MarketplaceService : IMarketplaceService
    {
        private readonly IMarketplaceRepository marketplaceRepository;
        private readonly DatabaseConnector dataBaseConnector;
        private User currentUser;

        /// <summary>
        /// Initializes a new instance of the <see cref="MarketplaceService"/> class.
        /// </summary>
        /// <param name="marketplaceRepository"> Marketplace repository. </param>
        /// <exception cref="ArgumentNullException"> Thrown if repository is null. </exception>
        public MarketplaceService(MarketplaceRepository marketplaceRepository)
        {
            if (marketplaceRepository != null)
            {
                this.marketplaceRepository = marketplaceRepository;
            }
            else
            {
                throw new ArgumentNullException(nameof(marketplaceRepository));
            }

            this.dataBaseConnector = new DatabaseConnector();
            this.currentUser = this.dataBaseConnector.GetCurrentUser();
        }

        /// <summary>
        /// Returns the current user.
        /// </summary>
        /// <returns> The current user. </returns>
        public User GetCurrentUser()
        {
            return this.currentUser;
        }

        /// <summary>
        /// Sets the current user to another.
        /// </summary>
        /// <param name="user"> User to be set. </param>
        /// <exception cref="ArgumentNullException"> Thrown if user is null. </exception>

        public void SetCurrentUser(User user)
        {
            if (user != null)
            {
                this.currentUser = user;
            }
            else
            {
                throw new ArgumentNullException(nameof(user));
            }
        }

        /// <summary>
        /// Retrieve all users from the database.
        /// </summary>
        /// <returns> A list of all Users. </returns>
        public List<User> GetAllUsers()
        {
            return this.dataBaseConnector.GetAllUsers();
        }

        /// <summary>
        /// Retrieves all the listings in the marketplace.
        /// </summary>
        /// <returns> A list of Items (marketplace listings). </returns>
        public List<Item> GetAllListings()
        {
            return this.marketplaceRepository.GetAllListedItems();
        }

        /// <summary>
        /// Retrieves listings from a certain game.
        /// </summary>
        /// <param name="game"> Game for which the listing is retrieved. </param>
        /// <returns> A list of Items (marketplace listings). </returns>
        /// <exception cref="ArgumentNullException"> Thrown if game is null. </exception>
        public List<Item> GetListingsByGame(Game game)
        {
            if (game == null)
            {
                throw new ArgumentNullException(nameof(game));
            }

            return this.marketplaceRepository.GetListedItemsByGame(game);
        }

        /// <summary>
        /// Adds listing (an item from a game).
        /// </summary>
        /// <param name="game"> Game from which the item to be listed is. </param>
        /// <param name="item"> Item to be added as listing. </param>
        /// <exception cref="ArgumentNullException"> Thrown if either the game or item is null. </exception>
        public void AddListing(Game game, Item item)
        {
            if (game == null)
            {
                throw new ArgumentNullException(nameof(game));
            }

            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            this.marketplaceRepository.MakeItemListable(game, item);
        }

        /// <summary>
        /// Remove listing (an item from a game).
        /// </summary>
        /// <param name="game"> Game from which the item to be listed is. </param>
        /// <param name="item"> Item listing to be removed. </param>
        /// <exception cref="ArgumentNullException"> Thrown if either the game or item is null. </exception>
        public void RemoveListing(Game game, Item item)
        {
            if (game == null)
            {
                throw new ArgumentNullException(nameof(game));
            }

            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            this.marketplaceRepository.MakeItemNotListable(game, item);
        }

        /// <summary>
        /// Update listing (an item from a game).
        /// </summary>
        /// <param name="game"> Game from which the item to be listed is. </param>
        /// <param name="item"> Item listing to be updated. </param>
        /// <exception cref="ArgumentNullException"> Thrown if either the game or item is null. </exception>
        public void UpdateListing(Game game, Item item)
        {
            if (game == null)
            {
                throw new ArgumentNullException(nameof(game));
            }

            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            this.marketplaceRepository.UpdateItemPrice(game, item);
        }

        /// <summary>
        /// Handles the purchase of an item by removing former ownership, adding the item to the new owner's inventory,
        /// and marking the item as unlisted.
        /// </summary>
        /// <param name="item"> Item to be purchased. </param>
        /// <returns> True upon successful purchase. </returns>
        /// <exception cref="ArgumentNullException"> Thrown when the item is null. </exception>
        /// <exception cref="InvalidOperationException"> Thrown when either the item is not listed or when the current user is null. </exception>
        public bool BuyItem(Item item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (!item.IsListed)
            {
                throw new InvalidOperationException("Item is not listed for sale");
            }

            if (this.currentUser == null)
            {
                throw new InvalidOperationException("No user selected");
            }

            return this.marketplaceRepository.BuyItem(item, this.currentUser);
        }
    }
}
