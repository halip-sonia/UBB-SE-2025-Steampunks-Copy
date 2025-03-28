using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Steampunks.Domain.Entities;
using System.Linq;
using System.Collections.Generic;
using Steampunks.Services;
using Steampunks.DataLink;

namespace Steampunks.ViewModels
{
    public class InventoryViewModel : INotifyPropertyChanged
    {
        private readonly InventoryService _inventoryService;
        private readonly DatabaseConnector _dbConnector;
        private ObservableCollection<Item> _inventoryItems;
        private ObservableCollection<Game> _availableGames;
        private ObservableCollection<User> _availableUsers;
        private Game _selectedGame;
        private User _selectedUser;
        private string _searchText;
        private List<Item> _allCurrentItems;
        private bool _isUpdating;
        private readonly Game _allGamesOption;
        private Item _selectedItem;

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

        public InventoryViewModel(DatabaseConnector dbConnector)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Starting InventoryViewModel initialization...");
                _dbConnector = dbConnector ?? throw new ArgumentNullException(nameof(dbConnector));
                System.Diagnostics.Debug.WriteLine("DatabaseConnector validated.");

                // Initialize collections first
                _inventoryItems = new ObservableCollection<Item>();
                _availableGames = new ObservableCollection<Game>();
                _availableUsers = new ObservableCollection<User>();
                _allCurrentItems = new List<Item>();
                System.Diagnostics.Debug.WriteLine("Collections initialized.");

                // Create the "All Games" option
                _allGamesOption = new Game("All Games", 0.0f, "", "Show items from all games");
                _availableGames.Add(_allGamesOption);
                _selectedGame = _allGamesOption;
                System.Diagnostics.Debug.WriteLine("All Games option created and added.");

                // Load users
                LoadUsers();
                System.Diagnostics.Debug.WriteLine("Users loaded successfully.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in InventoryViewModel constructor: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                // Initialize with empty collections if there's an error
                _inventoryItems = new ObservableCollection<Item>();
                _availableGames = new ObservableCollection<Game>();
                _availableUsers = new ObservableCollection<User>();
                _allCurrentItems = new List<Item>();
                _allGamesOption = new Game("All Games", 0.0f, "", "Show items from all games");
                _availableGames.Add(_allGamesOption);
                _selectedGame = _allGamesOption;
            }
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

        public ObservableCollection<User> AvailableUsers
        {
            get => _availableUsers;
            private set
            {
                if (_availableUsers != value)
                {
                    _availableUsers = value;
                    OnPropertyChanged();
                }
            }
        }

        public Game SelectedGame
        {
            get => _selectedGame;
            set
            {
                if (_selectedGame != value)
                {
                    _selectedGame = value;
                    OnPropertyChanged();
                    UpdateInventoryItems();
                }
            }
        }

        public User SelectedUser
        {
            get => _selectedUser;
            set
            {
                if (_selectedUser != value && !_isUpdating)
                {
                    _selectedUser = value;
                    OnPropertyChanged();
                    if (_selectedUser != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"User selected: {_selectedUser.Username}");
                        LoadInventoryItems();
                    }
                }
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged();
                    UpdateInventoryItems();
                }
            }
        }

        public Item SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (_selectedItem != value)
                {
                    _selectedItem = value;
                    OnPropertyChanged();
                }
            }
        }

        public void LoadInventoryItems()
        {
            if (_selectedUser == null) return;

            try
            {
                _isUpdating = true;
                System.Diagnostics.Debug.WriteLine($"Loading inventory for user: {_selectedUser.Username}");
                _allCurrentItems = _dbConnector.GetUserInventory(_selectedUser.UserId);
                System.Diagnostics.Debug.WriteLine($"Loaded {_allCurrentItems.Count} items");
                
                // Update available games - use Distinct() to prevent duplicates
                _availableGames.Clear();
                _availableGames.Add(_allGamesOption);
                var uniqueGames = _allCurrentItems.Select(i => i.Game).Distinct(new GameComparer());
                foreach (var game in uniqueGames)
                {
                    _availableGames.Add(game);
                    System.Diagnostics.Debug.WriteLine($"Added game to filter: {game.Title}");
                }
                
                UpdateInventoryItems();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading inventory items: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                _allCurrentItems = new List<Item>();
                _inventoryItems.Clear();
            }
            finally
            {
                _isUpdating = false;
            }
        }

        private void UpdateInventoryItems()
        {
            if (_isUpdating) return;

            try
            {
                _isUpdating = true;
                var filteredItems = _allCurrentItems.AsQueryable();

                if (_selectedGame != null && _selectedGame != _allGamesOption)
                {
                    filteredItems = filteredItems.Where(item => item.Game != null && item.Game.GameId == _selectedGame.GameId);
                }

                if (!string.IsNullOrWhiteSpace(_searchText))
                {
                    filteredItems = filteredItems.Where(item =>
                        item.ItemName.Contains(_searchText, StringComparison.OrdinalIgnoreCase) ||
                        item.Description.Contains(_searchText, StringComparison.OrdinalIgnoreCase));
                }

                _inventoryItems.Clear();
                foreach (var item in filteredItems)
                {
                    _inventoryItems.Add(item);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating inventory items: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                _inventoryItems.Clear();
            }
            finally
            {
                _isUpdating = false;
            }
        }

        private void LoadUsers()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Starting to load users...");
                var users = _dbConnector.GetAllUsers();
                System.Diagnostics.Debug.WriteLine($"Retrieved {users.Count} users from database.");
                
                _availableUsers.Clear();
                foreach (var user in users)
                {
                    _availableUsers.Add(user);
                }
                System.Diagnostics.Debug.WriteLine("Users added to AvailableUsers collection.");

                // Set default selected user to first user
                if (_availableUsers.Any())
                {
                    SelectedUser = _availableUsers.First();
                    System.Diagnostics.Debug.WriteLine($"Default user set to: {SelectedUser.Username}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("No users available in the database.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading users: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                _availableUsers.Clear();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private class GameComparer : IEqualityComparer<Game>
        {
            public bool Equals(Game x, Game y)
            {
                if (x == null || y == null)
                    return false;
                return x.GameId == y.GameId;
            }

            public int GetHashCode(Game obj)
            {
                if (obj == null)
                    return 0;
                return obj.GameId.GetHashCode();
            }
        }
    }
} 