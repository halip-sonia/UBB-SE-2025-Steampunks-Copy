// <copyright file="InventoryViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Steampunks.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using Steampunks.Domain.Entities;
    using Steampunks.Services.InventoryService.InventoryService;

    /// <summary>
    /// ViewModel for managing the inventory of items.
    /// </summary>
    public class InventoryViewModel : INotifyPropertyChanged
    {
        private readonly InventoryService inventoryService;
        private ObservableCollection<Item> inventoryItems;
        private ObservableCollection<Game> availableGames;
        private ObservableCollection<User> availableUsers;
        private Game selectedGame;
        private User selectedUser;
        private string searchText;
        private bool isUpdating;
        private Item selectedItem;

        /// <summary>
        /// Initializes a new instance of the <see cref="InventoryViewModel"/> class using the specified inventory service.
        /// </summary>
        /// <param name="inventoryService">The inventory service to use for data access and business logic.</param>
        public InventoryViewModel(InventoryService inventoryService)
        {
            this.inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
            this.inventoryItems = new ObservableCollection<Item>();
            this.availableGames = new ObservableCollection<Game>();
            this.availableUsers = new ObservableCollection<User>();

            // Load users and initialize data.
            this.LoadUsersAsync().GetAwaiter().GetResult();
        }

        /// <inheritdoc/>
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
                    // Update the filtered inventory when the game filter changes.
                    this.UpdateInventoryItemsAsync().ConfigureAwait(false);
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
                    // When a user is selected, load their inventory.
                    if (this.selectedUser != null)
                    {
                        _ = this.LoadInventoryItemsAsync();
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
                    // Update the filtered inventory when the search text changes.
                    this.UpdateInventoryItemsAsync().ConfigureAwait(false);
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
        /// Notifies listeners that a property value has changed.
        /// </summary>
        /// <param name="propertyName">
        /// Name of the property that changed.
        /// </param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Asynchronously loads the available users from the service.
        /// </summary>
        private async Task LoadUsersAsync()
        {
            try
            {
                var users = await this.inventoryService.GetAllUsersAsync();
                this.AvailableUsers.Clear();
                foreach (var user in users)
                {
                    this.AvailableUsers.Add(user);
                }

                // Set default selected user if available.
                if (this.AvailableUsers.Any())
                {
                    this.SelectedUser = this.AvailableUsers.First();
                }
            }
            catch (Exception ex)
            {
                // Log exception details as needed.
                System.Diagnostics.Debug.WriteLine($"Error loading users: {ex.Message}");
                this.AvailableUsers.Clear();
            }
        }

        /// <summary>
        /// Asynchronously loads the inventory items and available games for the selected user.
        /// </summary>
        public async Task LoadInventoryItemsAsync()
        {
            if (this.SelectedUser == null)
            {
                return;
            }

            try
            {
                this.isUpdating = true;

                // Retrieve filtered inventory based on the current game filter and search text.
                var filteredItems = await this.inventoryService.GetUserFilteredInventoryAsync(
                    this.SelectedUser.UserId,
                    this.SelectedGame,
                    this.SearchText);

                // Update the inventory items collection.
                this.InventoryItems.Clear();
                foreach (var item in filteredItems)
                {
                    this.InventoryItems.Add(item);
                }

                // Retrieve all inventory items to rebuild the games filter.
                var allItems = await this.inventoryService.GetUserInventoryAsync(this.SelectedUser.UserId);
                var games = this.inventoryService.GetAvailableGames(allItems);
                this.AvailableGames.Clear();
                foreach (var game in games)
                {
                    this.AvailableGames.Add(game);
                }
            }
            catch (Exception ex)
            {
                // Log exception details as needed.
                System.Diagnostics.Debug.WriteLine($"Error loading inventory items: {ex.Message}");
                this.InventoryItems.Clear();
            }
            finally
            {
                this.isUpdating = false;
            }
        }

        /// <summary>
        /// Updates the filtered inventory items based on the current game filter and search text.
        /// </summary>
        private async Task UpdateInventoryItemsAsync()
        {
            if (this.SelectedUser == null)
            {
                return;
            }

            try
            {
                this.isUpdating = true;

                var filteredItems = await this.inventoryService.GetUserFilteredInventoryAsync(
                    this.SelectedUser.UserId,
                    this.SelectedGame,
                    this.SearchText);

                this.InventoryItems.Clear();
                foreach (var item in filteredItems)
                {
                    this.InventoryItems.Add(item);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating inventory items: {ex.Message}");
                this.InventoryItems.Clear();
            }
            finally
            {
                this.isUpdating = false;
            }
        }

        public async Task<bool> SellItemAsync(Item selectedItem)
        {
            return await this.inventoryService.SellItemAsync(selectedItem);
        }
    }
}
