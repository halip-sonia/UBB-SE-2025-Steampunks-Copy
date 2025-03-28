using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Steampunks.Domain.Entities;
using Steampunks.Repository.Marketplace;
using Steampunks.ViewModels;
using Microsoft.Data.SqlClient;
using Steampunks.DataLink;

namespace Steampunks.Services
{
    public class MarketplaceService
    {
        private readonly MarketplaceRepository _marketplaceRepo;
        private readonly DatabaseConnector _dbConnector;
        private User _currentUser;

        public MarketplaceService(MarketplaceRepository marketplaceRepo)
        {
            _marketplaceRepo = marketplaceRepo ?? throw new ArgumentNullException(nameof(marketplaceRepo));
            _dbConnector = new DatabaseConnector();
            _currentUser = _dbConnector.GetCurrentUser();
        }

        public User GetCurrentUser()
        {
            return _currentUser;
        }

        public void SetCurrentUser(User user)
        {
            _currentUser = user ?? throw new ArgumentNullException(nameof(user));
        }

        public List<User> GetAllUsers()
        {
            return _dbConnector.GetAllUsers();
        }

        public List<Item> getAllListings()
        {
            return _marketplaceRepo.GetAllListings();
        }

        public List<Item> getListingsByGame(Game game)
        {
            if (game == null)
                throw new ArgumentNullException(nameof(game));

            return _marketplaceRepo.GetListingsByGame(game);
        }

        public void addListing(Game game, Item item)
        {
            if (game == null)
                throw new ArgumentNullException(nameof(game));
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            _marketplaceRepo.AddListing(game, item);
        }

        public void removeListing(Game game, Item item)
        {
            if (game == null)
                throw new ArgumentNullException(nameof(game));
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            _marketplaceRepo.RemoveListing(game, item);
        }

        public void updateListing(Game game, Item item)
        {
            if (game == null)
                throw new ArgumentNullException(nameof(game));
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            _marketplaceRepo.UpdateListing(game, item);
        }

        public bool BuyItem(Item item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            if (!item.IsListed)
                throw new InvalidOperationException("Item is not listed for sale");

            try
            {
                // Start transaction
                _dbConnector.OpenConnection();
                using (var transaction = _dbConnector.GetConnection().BeginTransaction())
                {
                    try
                    {
                        // Add item to user's inventory
                        using (var command = new SqlCommand(@"
                            INSERT INTO UserInventory (UserId, GameId, ItemId)
                            VALUES (@UserId, @GameId, @ItemId)", _dbConnector.GetConnection(), transaction))
                        {
                            command.Parameters.AddWithValue("@UserId", _currentUser.UserId);
                            command.Parameters.AddWithValue("@GameId", item.Game.GameId);
                            command.Parameters.AddWithValue("@ItemId", item.ItemId);
                            command.ExecuteNonQuery();
                        }

                        // Update item's listed status
                        using (var command = new SqlCommand(@"
                            UPDATE Items 
                            SET IsListed = 0
                            WHERE ItemId = @ItemId", _dbConnector.GetConnection(), transaction))
                        {
                            command.Parameters.AddWithValue("@ItemId", item.ItemId);
                            command.ExecuteNonQuery();
                        }

                        // Commit transaction
                        transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
            finally
            {
                _dbConnector.CloseConnection();
            }
        }
    }
}
