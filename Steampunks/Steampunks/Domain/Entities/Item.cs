using System;
using System.Diagnostics;

namespace Steampunks.Domain.Entities
{
    public class Item
    {
        private int _itemId;
        private string _itemName;
        private Game _game;
        private float _price;
        private string _description;
        private bool _isListed;
        private string _imagePath;

        public int ItemId
        {
            get => _itemId;
            set => _itemId = value;
        }

        public string ItemName
        {
            get => _itemName;
            set => _itemName = value;
        }

        public Game Game
        {
            get => _game;
            set => _game = value;
        }

        public float Price
        {
            get => _price;
            set => _price = value;
        }

        public string PriceDisplay
        {
            get => $"${_price:F2}";
        }

        public string Description
        {
            get => _description;
            set => _description = value;
        }

        public bool IsListed
        {
            get => _isListed;
            set => _isListed = value;
        }

        public string ImagePath
        {
            get => _imagePath;
            set => _imagePath = value;
        }

        private Item() { } // For EF Core

        public Item(string itemName, Game game, float price, string description)
        {
            _itemName = itemName ?? throw new ArgumentNullException(nameof(itemName));
            _game = game ?? throw new ArgumentNullException(nameof(game));
            _price = price;
            _description = description ?? throw new ArgumentNullException(nameof(description));
            _isListed = false;
            _imagePath = GetDefaultImagePath(itemName);
            Debug.WriteLine($"Created item {itemName} with default image path: {_imagePath}");
        }

        private string GetDefaultImagePath(string itemName)
        {
            // Extract the weapon type from the item name (before the | character)
            string weaponType = itemName.Contains("|") ? itemName.Split('|')[0].Trim().ToLower() : itemName.ToLower();
            
            // Return a path to a default image based on the weapon type
            var path = $"ms-appx:///Assets/img/games/cs2/{weaponType}.png";
            Debug.WriteLine($"Generated default path for {itemName}: {path}");
            return path;
        }

        public string GetItemName()
        {
            return _itemName;
        }

        public Game GetCorrespondingGame()
        {
            return _game;
        }

        public void SetItemDescription(string description)
        {
            _description = description;
        }

        public void SetItemId(int id)
        {
            _itemId = id;
        }

        public void SetIsListed(bool isListed)
        {
            _isListed = isListed;
        }

        public void SetPrice(float price)
        {
            _price = price;
        }

        public void SetImagePath(string imagePath)
        {
            Debug.WriteLine($"Setting image path for {_itemName}: {imagePath}");
            _imagePath = imagePath;
        }
    }
} 