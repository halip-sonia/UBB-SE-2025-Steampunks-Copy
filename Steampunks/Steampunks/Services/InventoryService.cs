using System;
using System.Collections.Generic;
using Steampunks.Domain.Entities;
using Steampunks.Repository.Inventory;

namespace Steampunks.Services
{
    public class InventoryService
    {
        private readonly InventoryRepository _inventoryRepo;
        private readonly User _user;

        public InventoryService(InventoryRepository inventoryRepo, User user)
        {
            _inventoryRepo = inventoryRepo ?? throw new ArgumentNullException(nameof(inventoryRepo));
            _user = user ?? throw new ArgumentNullException(nameof(user));
        }

        public List<Item> GetItemsFromInventory(Game game)
        {
            if (game == null)
                throw new ArgumentNullException(nameof(game));

            return _inventoryRepo.GetItemsFromInventory(game);
        }

        public List<Item> GetAllItemsFromInventory()
        {
            return _inventoryRepo.GetAllItemsFromInventory(_user);
        }

        public void AddItemToInventory(Game game, Item item)
        {
            if (game == null)
                throw new ArgumentNullException(nameof(game));
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            _inventoryRepo.AddItemToInventory(game, item, _user);
        }

        public void RemoveItemFromInventory(Game game, Item item)
        {
            if (game == null)
                throw new ArgumentNullException(nameof(game));
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            _inventoryRepo.RemoveItemFromInventory(game, item, _user);
        }
    }
} 