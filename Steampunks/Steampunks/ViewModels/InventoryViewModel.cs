using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Steampunks.Domain.Entities;
using System.Linq;
using System.Collections.Generic;
using Steampunks.Services;

namespace Steampunks.ViewModels
{
    public class InventoryViewModel : INotifyPropertyChanged
    {
        private readonly InventoryService _inventoryService;
        private ObservableCollection<Item> _inventoryItems;
        private ObservableCollection<Game> _availableGames;
        private Game _selectedGame;
        private string _searchText;
        private List<Item> _allItems;
        private bool _isUpdating;
        private readonly Game _allGamesOption;

        public event PropertyChangedEventHandler PropertyChanged;

        public InventoryViewModel(InventoryService inventoryService)
        {
            _inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
            _inventoryItems = new ObservableCollection<Item>();
            _availableGames = new ObservableCollection<Game>();
            _allItems = new List<Item>();
            // Create the "All Games" option
            _allGamesOption = new Game("All Games", 0.0f, "", "Show items from all games");
            InitializeData();
        }

        private void InitializeData()
        {
            // Get all items from inventory
            _allItems = _inventoryService.GetAllItemsFromInventory();
            
            // Get unique games from items
            var games = _allItems.Select(item => item.Game).Distinct().ToList();
            
            // Initialize available games with "All Games" option first
            _availableGames.Clear();
            _availableGames.Add(_allGamesOption); // Add "All Games" as the first option
            foreach (var game in games)
            {
                _availableGames.Add(game);
            }

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