namespace Steampunks.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using Microsoft.Data.SqlClient;
    using Steampunks.DataLink;
    using Steampunks.Domain.Entities;
    using Steampunks.Services;

    /// <summary>
    /// ViewModel for managing the inventory of items.
    /// </summary>
    public class InventoryViewModel : INotifyPropertyChanged
    {
        private readonly InventoryService inventoryService;
        private readonly DatabaseConnector dbConnector;
        private readonly Game allGamesOption;
        private ObservableCollection<Item> inventoryItems;
        private ObservableCollection<Game> availableGames;
        private ObservableCollection<User> availableUsers;
        private Game selectedGame;
        private User selectedUser;
        private string searchText;
        private List<Item> allCurrentItems;
        private bool isUpdating;
        private Item selectedItem;

        /// <summary>
        /// Initializes a new instance of the <see cref="InventoryViewModel"/> class using the specified inventory service.
        /// </summary>
        /// <param name="inventoryService">The inventory service to use for data access.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="inventoryService"/> is null.</exception>
        public InventoryViewModel(InventoryService inventoryService)
        {
            this.inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
            this.inventoryItems = new ObservableCollection<Item>();
            this.availableGames = new ObservableCollection<Game>();
            this.allCurrentItems = new List<Item>();

            // Create the "All Games" option.
            this.allGamesOption = new Game("All Games", 0.0f, string.Empty, "Show items from all games");

            // Synchronously call asynchronous initialization.
            this.InitializeDataAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InventoryViewModel"/> class using the specified database connector.
        /// </summary>
        /// <param name="dbConnector">The database connector to use for data operations.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="dbConnector"/> is null.</exception>
        public InventoryViewModel(DatabaseConnector dbConnector)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Starting InventoryViewModel initialization...");
                this.dbConnector = dbConnector ?? throw new ArgumentNullException(nameof(dbConnector));
                System.Diagnostics.Debug.WriteLine("DatabaseConnector validated.");

                // Initialize collections first.
                this.inventoryItems = new ObservableCollection<Item>();
                this.availableGames = new ObservableCollection<Game>();
                this.availableUsers = new ObservableCollection<User>();
                this.allCurrentItems = new List<Item>();
                System.Diagnostics.Debug.WriteLine("Collections initialized.");

                // Create the "All Games" option.
                this.allGamesOption = new Game("All Games", 0.0f, string.Empty, "Show items from all games");
                this.availableGames.Add(this.allGamesOption);
                this.selectedGame = this.allGamesOption;
                System.Diagnostics.Debug.WriteLine("All Games option created and added.");

                // Synchronously call asynchronous user loading.
                this.LoadUsersAsync().GetAwaiter().GetResult();
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

                // Initialize with empty collections if there's an error.
                this.inventoryItems = new ObservableCollection<Item>();
                this.availableGames = new ObservableCollection<Game>();
                this.availableUsers = new ObservableCollection<User>();
                this.allCurrentItems = new List<Item>();
                this.allGamesOption = new Game("All Games", 0.0f, string.Empty, "Show items from all games");
                this.availableGames.Add(this.allGamesOption);
                this.selectedGame = this.allGamesOption;
            }
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets the collection of inventory items.
        /// </summary>
        public ObservableCollection<Item> InventoryItems
        {
            get => this.inventoryItems;
            private set
            {
                if (this.inventoryItems != value)
                {
                    this.inventoryItems = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets the collection of available games.
        /// </summary>
        public ObservableCollection<Game> AvailableGames
        {
            get => this.availableGames;
            private set
            {
                if (this.availableGames != value)
                {
                    this.availableGames = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets the collection of available users.
        /// </summary>
        public ObservableCollection<User> AvailableUsers
        {
            get => this.availableUsers;
            private set
            {
                if (this.availableUsers != value)
                {
                    this.availableUsers = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the selected game filter.
        /// </summary>
        public Game SelectedGame
        {
            get => this.selectedGame;
            set
            {
                if (this.selectedGame != value)
                {
                    this.selectedGame = value;
                    this.OnPropertyChanged();
                    this.UpdateInventoryItems();
                }
            }
        }

        /// <summary>
        /// Gets or sets the selected user.
        /// </summary>
        public User SelectedUser
        {
            get => this.selectedUser;
            set
            {
                if (this.selectedUser != value && !this.isUpdating)
                {
                    this.selectedUser = value;
                    this.OnPropertyChanged();
                    if (this.selectedUser != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"User selected: {selectedUser.Username}");
                        // Fire and forget async call.
                        var _ = this.LoadInventoryItemsAsync();
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the search text used to filter inventory items.
        /// </summary>
        public string SearchText
        {
            get => this.searchText;
            set
            {
                if (this.searchText != value)
                {
                    this.searchText = value;
                    this.OnPropertyChanged();
                    this.UpdateInventoryItems();
                }
            }
        }

        /// <summary>
        /// Gets or sets the currently selected inventory item.
        /// </summary>
        public Item SelectedItem
        {
            get => this.selectedItem;
            set
            {
                if (this.selectedItem != value)
                {
                    this.selectedItem = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Asynchronously loads the inventory items for the selected user.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task LoadInventoryItemsAsync()
        {
            if (this.selectedUser == null)
            {
                return;
            }

            try
            {
                this.isUpdating = true;
                System.Diagnostics.Debug.WriteLine($"Loading inventory for user: {this.selectedUser.Username}");
                // Assuming an asynchronous method exists on dbConnector.
                this.allCurrentItems = await this.dbConnector.GetUserInventoryAsync(this.selectedUser.UserId);
                System.Diagnostics.Debug.WriteLine($"Loaded {this.allCurrentItems.Count} items");

                // Update available games - use Distinct() to prevent duplicates.
                this.availableGames.Clear();
                this.availableGames.Add(this.allGamesOption);
                var uniqueGames = this.allCurrentItems.Select(i => i.Game).Distinct(new GameComparer());
                foreach (var game in uniqueGames)
                {
                    this.availableGames.Add(game);
                    System.Diagnostics.Debug.WriteLine($"Added game to filter: {game.Title}");
                }

                this.UpdateInventoryItems();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading inventory items: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                this.allCurrentItems = new List<Item>();
                this.inventoryItems.Clear();
            }
            finally
            {
                this.isUpdating = false;
            }
        }

        /// <summary>
        /// Asynchronously sells the specified item by updating its listed status.
        /// </summary>
        /// <param name="item">The item to be sold.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a value indicating whether the operation succeeded.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="item"/> is null.</exception>
        public async Task<bool> SellItemAsync(Item item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            try
            {
                // Start transaction.
                await this.dbConnector.OpenConnectionAsync().ConfigureAwait(false);
                using (var transaction = this.dbConnector.GetConnection().BeginTransaction())
                {
                    try
                    {
                        // Update item's listed status.
                        using (var command = new SqlCommand(
                            @"
                            UPDATE Items 
                            SET IsListed = 1
                            WHERE ItemId = @ItemId", this.dbConnector.GetConnection(),
                            transaction))
                        {
                            command.Parameters.AddWithValue("@ItemId", item.ItemId);
                            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                        }

                        // Commit transaction asynchronously.
                        // (Assuming the underlying transaction supports asynchronous commit.)
                        await transaction.CommitAsync().ConfigureAwait(false);

                        // Refresh the inventory items after successful transaction.
                        await this.LoadInventoryItemsAsync().ConfigureAwait(false);

                        return true;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error in transaction: {ex.Message}");
                        System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                        transaction.Rollback();
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error selling item: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return false;
            }
            finally
            {
                this.dbConnector.CloseConnection();
            }
        }

        /// <summary>
        /// Notifies listeners that a property value has changed.
        /// </summary>
        /// <param name="propertyName">Name of the property that changed. This value is optional and will be automatically provided by the compiler.</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Asynchronously initializes the data for the inventory and available games.
        /// </summary>
        /// <returns>A task that represents the asynchronous initialization operation.</returns>
        private async Task InitializeDataAsync()
        {
            // Assuming an asynchronous method exists on inventoryService.
            this.allCurrentItems = await this.inventoryService.GetAllItemsFromInventoryAsync();
            var games = this.allCurrentItems.Select(item => item.Game).Distinct().ToList();

            // Initialize available games with "All Games" option first.
            this.availableGames.Clear();
            this.availableGames.Add(this.allGamesOption); // Add "All Games" as the first option.
            foreach (var game in games)
            {
                this.availableGames.Add(game);
            }

            // Set initial selection to "All Games".
            this.SelectedGame = this.allGamesOption;
        }

        /// <summary>
        /// Updates the inventory items based on the selected game and search text.
        /// </summary>
        private void UpdateInventoryItems()
        {
            if (this.isUpdating)
            {
                return;
            }

            try
            {
                this.isUpdating = true;
                var filteredItems = this.allCurrentItems.AsQueryable();

                // Filter out listed items.
                filteredItems = filteredItems.Where(item => !item.IsListed);

                // Apply game filter if a specific game is selected.
                if (this.selectedGame != null && this.selectedGame != this.allGamesOption)
                {
                    filteredItems = filteredItems.Where(item => item.Game != null && item.Game.GameId == this.selectedGame.GameId);
                }

                // Apply search filter if there's search text.
                if (!string.IsNullOrWhiteSpace(this.searchText))
                {
                    filteredItems = filteredItems.Where(item =>
                        item.ItemName.Contains(this.searchText, StringComparison.OrdinalIgnoreCase) ||
                        item.Description.Contains(this.searchText, StringComparison.OrdinalIgnoreCase));
                }

                this.inventoryItems.Clear();
                foreach (var item in filteredItems)
                {
                    this.inventoryItems.Add(item);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating inventory items: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                this.inventoryItems.Clear();
            }
            finally
            {
                this.isUpdating = false;
            }
        }

        /// <summary>
        /// Asynchronously loads the available users from the database.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private async Task LoadUsersAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Starting to load users...");
                // Assuming an asynchronous method exists on dbConnector.
                var users = await this.dbConnector.GetAllUsersAsync();
                System.Diagnostics.Debug.WriteLine($"Retrieved {users.Count} users from database.");

                this.availableUsers.Clear();
                foreach (var user in users)
                {
                    this.availableUsers.Add(user);
                }

                System.Diagnostics.Debug.WriteLine("Users added to AvailableUsers collection.");

                // Set default selected user to first user.
                if (this.availableUsers.Any())
                {
                    this.SelectedUser = this.availableUsers.First();
                    System.Diagnostics.Debug.WriteLine($"Default user set to: {this.SelectedUser.Username}");
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

                this.availableUsers.Clear();
            }
        }

        /// <summary>
        /// Loads the available games based on the current items.
        /// </summary>
        private void LoadAvailableGames()
        {
            try
            {
                this.availableGames.Clear();
                this.availableGames.Add(this.allGamesOption);

                var games = this.allCurrentItems
                    .Select(item => item.Game)
                    .Where(game => game != null)
                    .Distinct(new GameComparer());

                foreach (var game in games)
                {
                    this.availableGames.Add(game);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading available games: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Provides a custom comparer for <see cref="Game"/> objects based on the GameId.
        /// </summary>
        private class GameComparer : IEqualityComparer<Game>
        {
            /// <summary>
            /// Determines whether two <see cref="Game"/> instances are equal.
            /// </summary>
            /// <param name="x">The first game to compare.</param>
            /// <param name="y">The second game to compare.</param>
            /// <returns><c>true</c> if the specified games are equal; otherwise, <c>false</c>.</returns>
            public bool Equals(Game x, Game y)
            {
                if (x == null || y == null)
                {
                    return false;
                }

                return x.GameId == y.GameId;
            }

            /// <summary>
            /// Returns a hash code for the specified <see cref="Game"/>.
            /// </summary>
            /// <param name="obj">The game for which a hash code is to be returned.</param>
            /// <returns>A hash code for the specified game.</returns>
            public int GetHashCode(Game obj)
            {
                if (obj == null)
                {
                    return 0;
                }

                return obj.GameId.GetHashCode();
            }
        }
    }
}
