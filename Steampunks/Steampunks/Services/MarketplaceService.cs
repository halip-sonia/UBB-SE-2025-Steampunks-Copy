using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Steampunks.Domain.Entities;
using Steampunks.Repository.Marketplace;
using Steampunks.ViewModels;
using Microsoft.Data.SqlClient;
using Steampunks.DataLink;
using System.Diagnostics;

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
            var items = _marketplaceRepo.GetAllListings();
            foreach (var item in items)
            {
                // The GetDefaultImagePath method will be called automatically in the Item constructor
                // but we can force a refresh here if needed
                var imagePath = item.ImagePath;
                Debug.WriteLine($"Loaded item {item.ItemName} with image path: {imagePath}");
            }
            return items;
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

            if (_currentUser == null)
                throw new InvalidOperationException("No user selected");

            try
            {
                // Start transaction
                _dbConnector.OpenConnection();
                using (var transaction = _dbConnector.GetConnection().BeginTransaction())
                {
                    try
                    {
                        // Get the current owner's ID
                        int currentOwnerId;
                        using (var command = new SqlCommand(@"
                            SELECT UserId 
                            FROM UserInventory 
                            WHERE ItemId = @ItemId", _dbConnector.GetConnection(), transaction))
                        {
                            command.Parameters.AddWithValue("@ItemId", item.ItemId);
                            currentOwnerId = (int)command.ExecuteScalar();
                        }

                        // Remove item from current owner's inventory
                        using (var command = new SqlCommand(@"
                            DELETE FROM UserInventory 
                            WHERE ItemId = @ItemId", _dbConnector.GetConnection(), transaction))
                        {
                            command.Parameters.AddWithValue("@ItemId", item.ItemId);
                            command.ExecuteNonQuery();
                        }

                        // Add item to buyer's inventory
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
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error in BuyItem transaction: {ex.Message}");
                        System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                        transaction.Rollback();
                        throw new InvalidOperationException("Failed to complete purchase. Please try again.");
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
