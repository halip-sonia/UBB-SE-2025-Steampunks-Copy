// <copyright file="TradeViewModel.cs" company="PlaceholderCompany">
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
    using Steampunks.DataLink;
    using Steampunks.Domain.Entities;
    using Steampunks.Services;

    /// <summary>
    /// The ViewModel for game and item Trade.
    /// </summary>
    public partial class TradeViewModel : INotifyPropertyChanged, ITradeViewModel
    {
        private readonly ITradeService tradeService;
        private readonly UserService userService;
        private readonly GameService gameService;
        private readonly DatabaseConnector dbConnector;
        private ObservableCollection<Item> sourceUserItems;
        private ObservableCollection<Item> destinationUserItems;
        private ObservableCollection<Item> selectedSourceItems;
        private ObservableCollection<Item> selectedDestinationItems;
        private ObservableCollection<ItemTrade> activeTrades;
        private ObservableCollection<ItemTrade> tradeHistory;
        private User? currentUser;
        private User? selectedUser;
        private Game? selectedGame;
        private string? tradeDescription;
        private ItemTrade? selectedTrade;
        private ObservableCollection<Game> games;
        private ObservableCollection<User> users;

        /// <summary>
        /// Initializes a new instance of the <see cref="TradeViewModel"/> class.
        /// </summary>
        /// <param name="tradeService">The service for trading operations.</param>
        /// <param name="userService">The service for user operations.</param>
        /// <param name="gameService">The service for game operations.</param>
        /// <param name="dbConnector">The connector to the database.</param>
        public TradeViewModel(ITradeService tradeService, UserService userService, GameService gameService, DatabaseConnector dbConnector)
        {
            this.tradeService = tradeService;
            this.userService = userService;
            this.gameService = gameService;
            this.dbConnector = dbConnector ?? throw new ArgumentNullException(nameof(dbConnector));
            this.sourceUserItems = new ObservableCollection<Item>();
            this.destinationUserItems = new ObservableCollection<Item>();
            this.selectedSourceItems = new ObservableCollection<Item>();
            this.selectedDestinationItems = new ObservableCollection<Item>();
            this.activeTrades = new ObservableCollection<ItemTrade>();
            this.tradeHistory = new ObservableCollection<ItemTrade>();
            this.users = new ObservableCollection<User>();
            this.games = new ObservableCollection<Game>();
            this.LoadInitialData();
        }

        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <inheritdoc/>
        public ObservableCollection<User> Users
        {
            get => this.users;
            set
            {
                if (this.users != value)
                {
                    this.users = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <inheritdoc/>
        public ObservableCollection<Game> Games
        {
            get => this.games;
            set
            {
                if (this.games != value)
                {
                    this.games = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <inheritdoc/>
        public ObservableCollection<Item> SourceUserItems
        {
            get => this.sourceUserItems;
            set
            {
                if (this.sourceUserItems != value)
                {
                    this.sourceUserItems = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <inheritdoc/>
        public ObservableCollection<Item> DestinationUserItems
        {
            get => this.destinationUserItems;
            set
            {
                if (this.destinationUserItems != value)
                {
                    this.destinationUserItems = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <inheritdoc/>
        public ObservableCollection<Item> SelectedSourceItems
        {
            get => this.selectedSourceItems;
            set
            {
                if (this.selectedSourceItems != value)
                {
                    this.selectedSourceItems = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <inheritdoc/>
        public ObservableCollection<Item> SelectedDestinationItems
        {
            get => this.selectedDestinationItems;
            set
            {
                if (this.selectedDestinationItems != value)
                {
                    this.selectedDestinationItems = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <inheritdoc/>
        public ObservableCollection<ItemTrade> ActiveTrades
        {
            get => this.activeTrades;
            private set
            {
                this.activeTrades = value;
                this.OnPropertyChanged();
            }
        }

        /// <inheritdoc/>
        public ObservableCollection<ItemTrade> TradeHistory
        {
            get => this.tradeHistory;
            private set
            {
                this.tradeHistory = value;
                this.OnPropertyChanged();
            }
        }

        /// <inheritdoc/>
        public ObservableCollection<User> AvailableUsers
        {
            get
            {
                if (this.CurrentUser == null)
                {
                    return new ObservableCollection<User>(this.Users);
                }

                return new ObservableCollection<User>(this.Users.Where(u => u.UserId != this.CurrentUser.UserId));
            }
        }

        /// <inheritdoc/>
        public User? CurrentUser
        {
            get => this.currentUser;
            set
            {
                if (this.currentUser != value)
                {
                    this.currentUser = value;
                    this.OnPropertyChanged();
                    this.OnPropertyChanged(nameof(this.AvailableUsers));
                    this.LoadUserInventory();
                    this.LoadActiveTrades();
                    this.LoadTradeHistory();
                }
            }
        }

        /// <inheritdoc/>
        public User? SelectedUser
        {
            get => this.selectedUser;
            set
            {
                if (this.selectedUser != value)
                {
                    this.selectedUser = value;
                    this.OnPropertyChanged();
                    this.LoadDestinationUserInventory();
                }
            }
        }

        /// <inheritdoc/>
        public Game? SelectedGame
        {
            get => this.selectedGame;
            set
            {
                if (this.selectedGame != value)
                {
                    this.selectedGame = value;
                    this.OnPropertyChanged();
                    if (this.CurrentUser != null)
                    {
                        this.LoadUserInventory();
                    }

                    if (this.SelectedUser != null)
                    {
                        this.LoadDestinationUserInventory();
                    }
                }
            }
        }

        /// <inheritdoc/>
        public string? TradeDescription
        {
            get => this.tradeDescription;
            set
            {
                if (this.tradeDescription != value)
                {
                    this.tradeDescription = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <inheritdoc/>
        public ItemTrade? SelectedTrade
        {
            get => this.selectedTrade;
            set
            {
                this.selectedTrade = value;
                this.OnPropertyChanged(nameof(this.SelectedTrade));
                this.OnPropertyChanged(nameof(this.CanAcceptOrDeclineTrade));
            }
        }

        /// <inheritdoc/>
        public bool CanAcceptOrDeclineTrade => this.SelectedTrade != null;

        /// <inheritdoc/>
        public bool CanSendTradeOffer
        {
            get
            {
                return this.CurrentUser != null &&
                       this.SelectedUser != null &&
                       this.CurrentUser.UserId != this.SelectedUser.UserId &&
                       (this.SelectedSourceItems.Count > 0 || this.SelectedDestinationItems.Count > 0) &&
                       !string.IsNullOrWhiteSpace(this.TradeDescription);
            }
        }

        /// <inheritdoc/>
        public void AddSourceItem(Item item)
        {
            if (item != null && !this.SelectedSourceItems.Contains(item))
            {
                this.SelectedSourceItems.Add(item);
                this.SourceUserItems.Remove(item);
                this.OnPropertyChanged(nameof(this.CanSendTradeOffer));
            }
        }

        /// <inheritdoc/>
        public void RemoveSourceItem(Item item)
        {
            if (item != null)
            {
                this.SelectedSourceItems.Remove(item);
                this.SourceUserItems.Add(item);
                this.OnPropertyChanged(nameof(this.CanSendTradeOffer));
            }
        }

        /// <inheritdoc/>
        public void AddDestinationItem(Item item)
        {
            if (item != null && !this.SelectedDestinationItems.Contains(item))
            {
                this.SelectedDestinationItems.Add(item);
                this.DestinationUserItems.Remove(item);
                this.OnPropertyChanged(nameof(this.CanSendTradeOffer));
            }
        }

        /// <inheritdoc/>
        public void RemoveDestinationItem(Item item)
        {
            if (item != null)
            {
                this.SelectedDestinationItems.Remove(item);
                this.DestinationUserItems.Add(item);
                this.OnPropertyChanged(nameof(this.CanSendTradeOffer));
            }
        }

        /// <inheritdoc/>
        public async Task CreateTradeOffer()
        {
            if (!this.CanSendTradeOffer)
            {
                return;
            }

            try
            {
                if (this.CurrentUser == null || this.SelectedUser == null || this.SelectedGame == null || this.TradeDescription == null)
                {
                    throw new NullReferenceException();
                }

                var trade = new ItemTrade(this.CurrentUser, this.SelectedUser, this.SelectedGame, this.TradeDescription);

                foreach (var item in this.SelectedSourceItems)
                {
                    trade.AddSourceUserItem(item);
                }

                foreach (var item in this.SelectedDestinationItems)
                {
                    trade.AddDestinationUserItem(item);
                }

                this.dbConnector.CreateItemTrade(trade);
                await this.LoadActiveTradesAsync();

                // Clear selections
                this.SelectedSourceItems.Clear();
                this.SelectedDestinationItems.Clear();
                this.TradeDescription = string.Empty;
                this.LoadUserInventory();
                this.LoadDestinationUserInventory();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating trade offer: {ex.Message}");
            }
        }

        /// <inheritdoc/>
        public async void AcceptTrade(ItemTrade trade)
        {
            try
            {
                if (this.CurrentUser == null)
                {
                    throw new NullReferenceException();
                }

                bool isSourceUser = trade.SourceUser.UserId == this.CurrentUser.UserId;
                await this.tradeService.AcceptTradeAsync(trade, isSourceUser);

                // Clear the selected trade
                this.SelectedTrade = null;

                // Refresh all relevant data
                this.LoadActiveTrades();
                this.LoadTradeHistory();
                this.LoadUserInventory();
                this.LoadDestinationUserInventory();

                // Notify UI of changes
                this.OnPropertyChanged(nameof(this.ActiveTrades));
                this.OnPropertyChanged(nameof(this.TradeHistory));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error accepting trade: {ex.Message}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }

                throw;
            }
        }

        /// <inheritdoc/>
        public Task<bool> DeclineTrade(ItemTrade trade)
        {
            if (trade == null)
            {
                return Task.FromResult(false);
            }

            try
            {
                trade.Decline();
                this.dbConnector.UpdateItemTrade(trade);

                // Clear the selected trade
                this.SelectedTrade = null;

                // Refresh all relevant data
                this.LoadActiveTrades();
                this.LoadTradeHistory();

                // Notify UI of changes
                this.OnPropertyChanged(nameof(this.ActiveTrades));
                this.OnPropertyChanged(nameof(this.TradeHistory));

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error declining trade: {ex.Message}");
                return Task.FromResult(false);
            }
        }

        /// <inheritdoc/>
        public async Task LoadUsersAsync()
        {
            try
            {
                // var users = this.dbConnector.GetAllUsers();
                var users = await this.userService.GetAllUsersAsync();

                this.Users.Clear();
                foreach (var user in users)
                {
                    this.Users.Add(user);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading users: {ex.Message}");
            }
        }

        /// <inheritdoc/>
        public async Task LoadGamesAsync()
        {
            try
            {
                // var games = this.dbConnector.GetGamesAsync();
                var games = await this.gameService.GetAllGamesAsync();

                this.Games.Clear();
                foreach (var game in games)
                {
                    this.Games.Add(game);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading games: {ex.Message}");
            }
        }

        /// <summary>
        /// Handler for property changes.
        /// </summary>
        /// <param name="propertyName">The property.</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async void LoadInitialData()
        {
            await this.LoadUsersAsync();
            await this.LoadGamesAsync();
            this.LoadCurrentUser();
            this.LoadActiveTrades();
        }

        private void LoadCurrentUser()
        {
            try
            {
                this.CurrentUser = this.dbConnector.GetCurrentUser();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading current user: {ex.Message}");
            }
        }

        private void LoadUserInventory()
        {
            if (this.CurrentUser == null)
            {
                return;
            }

            try
            {
                var items = this.dbConnector.GetUserInventory(this.CurrentUser.UserId);
                this.SourceUserItems.Clear();
                foreach (var item in items.Where(i => !i.IsListed))
                {
                    if (this.SelectedGame == null || item.Game.GameId == this.SelectedGame.GameId)
                    {
                        this.SourceUserItems.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading user inventory: {ex.Message}");
            }
        }

        private void LoadDestinationUserInventory()
        {
            if (this.SelectedUser == null)
            {
                return;
            }

            try
            {
                var items = this.dbConnector.GetUserInventory(this.SelectedUser.UserId);
                this.DestinationUserItems.Clear();
                foreach (var item in items.Where(i => !i.IsListed))
                {
                    if (this.SelectedGame == null || item.Game.GameId == this.SelectedGame.GameId)
                    {
                        this.DestinationUserItems.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading destination user inventory: {ex.Message}");
            }
        }

        private async Task LoadActiveTradesAsync()
        {
            if (this.CurrentUser == null)
            {
                return;
            }

            try
            {
                var trades = await this.tradeService.GetActiveTradesAsync(this.CurrentUser.UserId);
                this.ActiveTrades.Clear();
                foreach (var trade in trades)
                {
                    this.ActiveTrades.Add(trade);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading active trades: {ex.Message}");
            }
        }

        private void LoadActiveTrades()
        {
            if (this.CurrentUser == null)
            {
                return;
            }

            try
            {
                var trades = this.dbConnector.GetActiveItemTrades(this.CurrentUser.UserId);
                this.ActiveTrades.Clear();
                foreach (var trade in trades)
                {
                    this.ActiveTrades.Add(trade);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading active trades: {ex.Message}");
            }
        }

        private void LoadTradeHistory()
        {
            if (this.CurrentUser == null)
            {
                return;
            }

            try
            {
                var trades = this.dbConnector.GetItemTradeHistory(this.CurrentUser.UserId);
                this.TradeHistory.Clear();
                foreach (var trade in trades)
                {
                    // Only add trades where the current user is involved
                    if (trade.SourceUser.UserId == this.CurrentUser.UserId ||
                        trade.DestinationUser.UserId == this.CurrentUser.UserId)
                    {
                        this.TradeHistory.Add(trade);
                    }
                }

                this.OnPropertyChanged(nameof(this.TradeHistory));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading trade history: {ex.Message}");
            }
        }
    }
}
