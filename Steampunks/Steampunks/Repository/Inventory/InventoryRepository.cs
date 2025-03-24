using System;
using System.Collections.Generic;
using Steampunks.Domain.Entities;
using Steampunks.DataLink;

namespace Steampunks.Repository.Inventory
{
    public class InventoryRepository : IInventoryRepository
    {
        private readonly DatabaseConnector _dbConnector;

        public InventoryRepository()
        {
            _dbConnector = new DatabaseConnector();
        }

        public List<Item> GetItemsFromInventory(Game game)
        {
            if (game == null)
                throw new ArgumentNullException(nameof(game));

            return _dbConnector.GetInventoryItems(game);
        }

        public List<Item> GetAllItemsFromInventory(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            return _dbConnector.GetAllInventoryItems(user);
        }

        public void AddItemToInventory(Game game, Item item, User user)
        {
            if (game == null)
                throw new ArgumentNullException(nameof(game));
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            _dbConnector.AddInventoryItem(game, item, user);
        }

        public void RemoveItemFromInventory(Game game, Item item, User user)
        {
            if (game == null)
                throw new ArgumentNullException(nameof(game));
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            _dbConnector.RemoveInventoryItem(game, item, user);
        }
    }

    public interface IInventoryRepository
    {
        List<Item> GetItemsFromInventory(Game game);
        List<Item> GetAllItemsFromInventory(User user);
        void AddItemToInventory(Game game, Item item, User user);
        void RemoveItemFromInventory(Game game, Item item, User user);
    }
} 