using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Steampunks.Domain.Entities;
using System.Linq;
using System.Collections.Generic;

namespace Steampunks.ViewModels
{
    public class InventoryViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<Item> _inventoryItems;
        private ObservableCollection<Game> _availableGames;
        private Game _selectedGame;
        private string _searchText;
        private List<Item> _allItems;
        private bool _isUpdating;
        private readonly Game _allGamesOption;

        public event PropertyChangedEventHandler PropertyChanged;

        public InventoryViewModel()
        {
            _inventoryItems = new ObservableCollection<Item>();
            _availableGames = new ObservableCollection<Game>();
            _allItems = new List<Item>();
            // Create the "All Games" option
            _allGamesOption = new Game("All Games", 0.0f, "", "Show items from all games");
            InitializeHardcodedData();
        }

        private void InitializeHardcodedData()
        {
            // Create some sample games
            var csgo = new Game("Counter-Strike 2", 0.0f, "FPS", "The next evolution of CS");
            var dota = new Game("Dota 2", 0.0f, "MOBA", "A complex MOBA game");
            var tf2 = new Game("Team Fortress 2", 0.0f, "FPS", "Classic team-based action");

            // Initialize available games with "All Games" option first
            _availableGames.Clear();
            _availableGames.Add(_allGamesOption); // Add "All Games" as the first option
            foreach (var game in new[] { csgo, dota, tf2 })
            {
                _availableGames.Add(game);
            }

            // Create sample items for each game
            _allItems.Clear();
            _allItems.AddRange(new[]
            {
                // CS2 Items
                new Item("AK-47 | Asiimov", csgo, 50.0f, "A sci-fi themed skin for the AK-47"),
                new Item("M4A4 | Howl", csgo, 1500.0f, "A rare and coveted M4A4 skin"),
                new Item("AWP | Dragon Lore", csgo, 10000.0f, "The legendary Dragon Lore skin"),
                new Item("Desert Eagle | Blaze", csgo, 450.0f, "A blazing hot Desert Eagle skin"),
                
                // Dota 2 Items
                new Item("Dragonclaw Hook", dota, 750.0f, "A legendary hook for Pudge"),
                new Item("Arcana: Demon Eater", dota, 35.0f, "Special effects for Shadow Fiend"),
                new Item("Baby Roshan", dota, 150.0f, "A cute courier skin"),
                
                // TF2 Items
                new Item("Unusual Team Captain", tf2, 200.0f, "A rare hat with special effects"),
                new Item("Australium Rocket Launcher", tf2, 100.0f, "Golden rocket launcher"),
                new Item("Mann Co. Supply Crate Key", tf2, 2.5f, "Used to open crates")
            });

            // Set initial selection to "All Games"
            SelectedGame = _allGamesOption;
        }

        public ObservableCollection<Item> InventoryItems
        {
            get => _inventoryItems;
            private set
            {
                if (_inventoryItems != value)
                {
                    _inventoryItems = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<Game> AvailableGames
        {
            get => _availableGames;
            private set
            {
                if (_availableGames != value)
                {
                    _availableGames = value;
                    OnPropertyChanged();
                }
            }
        }

        public Game SelectedGame
        {
            get => _selectedGame;
            set
            {
                if (_selectedGame != value && !_isUpdating)
                {
                    _selectedGame = value;
                    OnPropertyChanged();
                    UpdateInventoryItems();
                }
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value && !_isUpdating)
                {
                    _searchText = value;
                    OnPropertyChanged();
                    UpdateInventoryItems();
                }
            }
        }

        private void UpdateInventoryItems()
        {
            try
            {
                _isUpdating = true;
                IEnumerable<Item> filteredItems = _allItems;

                // Filter by selected game, but only if not "All Games"
                if (SelectedGame != null && SelectedGame != _allGamesOption)
                {
                    filteredItems = filteredItems.Where(item => 
                        item.Game.Title == SelectedGame.Title);
                }

                // Filter by search text
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    var searchLower = SearchText.ToLower();
                    filteredItems = filteredItems.Where(item =>
                        item.ItemName.ToLower().Contains(searchLower) ||
                        item.Description.ToLower().Contains(searchLower));
                }

                // Update the display
                _inventoryItems.Clear();
                foreach (var item in filteredItems.ToList())
                {
                    _inventoryItems.Add(item);
                }
            }
            finally
            {
                _isUpdating = false;
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (!_isUpdating)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
} 