using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Steampunks.Domain.Entities;
using Steampunks.Services;
using Steampunks.DataLink;
using System.Linq;

namespace Steampunks.ViewModels
{
    public class TradeViewModel : INotifyPropertyChanged
    {
        private readonly TradeService _tradeService;
        private readonly UserService _userService;
        private readonly GameService _gameService;
        private readonly DatabaseConnector _dbConnector;
        private ObservableCollection<Item> _sourceUserItems;
        private ObservableCollection<Item> _destinationUserItems;
        private ObservableCollection<Item> _selectedSourceItems;
        private ObservableCollection<Item> _selectedDestinationItems;
        private ObservableCollection<ItemTrade> _activeTrades;
        private ObservableCollection<ItemTrade> _tradeHistory;
        private User _currentUser;
        private User _selectedUser;
        private Game _selectedGame;
        private string _tradeDescription;
        private ItemTrade _selectedTrade;

        private ObservableCollection<User> _users;
        public ObservableCollection<User> Users
        {
            get => _users;
            set
            {
                if (_users != value)
                {
                    _users = value;
                    OnPropertyChanged();
                }
            }
        }

        private ObservableCollection<Game> _games;
        public ObservableCollection<Game> Games
        {
            get => _games;
            set
            {
                if (_games != value)
                {
                    _games = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public TradeViewModel(TradeService tradeService, UserService userService, GameService gameService, DatabaseConnector dbConnector)
        {
            _tradeService = tradeService;
            _userService = userService;
            _gameService = gameService;
            _dbConnector = dbConnector ?? throw new ArgumentNullException(nameof(dbConnector));
            _sourceUserItems = new ObservableCollection<Item>();
            _destinationUserItems = new ObservableCollection<Item>();
            _selectedSourceItems = new ObservableCollection<Item>();
            _selectedDestinationItems = new ObservableCollection<Item>();
            _activeTrades = new ObservableCollection<ItemTrade>();
            _tradeHistory = new ObservableCollection<ItemTrade>();
            Users = new ObservableCollection<User>();
            Games = new ObservableCollection<Game>();
            LoadInitialData();
        }

        private async void LoadInitialData()
        {
            await LoadUsersAsync();
            await LoadGamesAsync();
            LoadCurrentUser();
            LoadActiveTrades();
        }

        public ObservableCollection<Item> SourceUserItems
        {
            get => _sourceUserItems;
            set
            {
                if (_sourceUserItems != value)
                {
                    _sourceUserItems = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<Item> DestinationUserItems
        {
            get => _destinationUserItems;
            set
            {
                if (_destinationUserItems != value)
                {
                    _destinationUserItems = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<Item> SelectedSourceItems
        {
            get => _selectedSourceItems;
            set
            {
                if (_selectedSourceItems != value)
                {
                    _selectedSourceItems = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<Item> SelectedDestinationItems
        {
            get => _selectedDestinationItems;
            set
            {
                if (_selectedDestinationItems != value)
                {
                    _selectedDestinationItems = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<ItemTrade> ActiveTrades
        {
            get => _activeTrades;
            private set
            {
                _activeTrades = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<ItemTrade> TradeHistory
        {
            get => _tradeHistory;
            private set
            {
                _tradeHistory = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<User> AvailableUsers
        {
            get
            {
                if (CurrentUser == null) return new ObservableCollection<User>(Users);
                return new ObservableCollection<User>(Users.Where(u => u.UserId != CurrentUser.UserId));
            }
        }

        public User CurrentUser
        {
            get => _currentUser;
            set
            {
                if (_currentUser != value)
                {
                    _currentUser = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(AvailableUsers));
                    LoadUserInventory();
                    LoadActiveTrades();
                }
            }
        }

        public User SelectedUser
        {
            get => _selectedUser;
            set
            {
                if (_selectedUser != value)
                {
                    _selectedUser = value;
                    OnPropertyChanged();
                    LoadDestinationUserInventory();
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
                    if (CurrentUser != null)
                    {
                        LoadUserInventory();
                    }
                    if (SelectedUser != null)
                    {
                        LoadDestinationUserInventory();
                    }
                }
            }
        }

        public string TradeDescription
        {
            get => _tradeDescription;
            set
            {
                if (_tradeDescription != value)
                {
                    _tradeDescription = value;
                    OnPropertyChanged();
                }
            }
        }

        public ItemTrade SelectedTrade
        {
            get => _selectedTrade;
            set
            {
                _selectedTrade = value;
                OnPropertyChanged();
            }
        }

        public bool CanSendTradeOffer
        {
            get
            {
                return CurrentUser != null && 
                       SelectedUser != null && 
                       CurrentUser.UserId != SelectedUser.UserId &&
                       (SelectedSourceItems.Count > 0 || SelectedDestinationItems.Count > 0) &&
                       !string.IsNullOrWhiteSpace(TradeDescription);
            }
        }

        private void LoadCurrentUser()
        {
            try
            {
                CurrentUser = _dbConnector.GetCurrentUser();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading current user: {ex.Message}");
            }
        }

        private void LoadUserInventory()
        {
            if (CurrentUser == null) return;

            try
            {
                var items = _dbConnector.GetUserInventory(CurrentUser.UserId);
                SourceUserItems.Clear();
                foreach (var item in items.Where(i => !i.IsListed))
                {
                    if (SelectedGame == null || item.Game.GameId == SelectedGame.GameId)
                    {
                        SourceUserItems.Add(item);
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
            if (SelectedUser == null) return;

            try
            {
                var items = _dbConnector.GetUserInventory(SelectedUser.UserId);
                DestinationUserItems.Clear();
                foreach (var item in items.Where(i => !i.IsListed))
                {
                    if (SelectedGame == null || item.Game.GameId == SelectedGame.GameId)
                    {
                        DestinationUserItems.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading destination user inventory: {ex.Message}");
            }
        }

        public void AddSourceItem(Item item)
        {
            if (item != null && !SelectedSourceItems.Contains(item))
            {
                SelectedSourceItems.Add(item);
                SourceUserItems.Remove(item);
                OnPropertyChanged(nameof(CanSendTradeOffer));
            }
        }

        public void RemoveSourceItem(Item item)
        {
            if (item != null)
            {
                SelectedSourceItems.Remove(item);
                SourceUserItems.Add(item);
                OnPropertyChanged(nameof(CanSendTradeOffer));
            }
        }

        public void AddDestinationItem(Item item)
        {
            if (item != null && !SelectedDestinationItems.Contains(item))
            {
                SelectedDestinationItems.Add(item);
                DestinationUserItems.Remove(item);
                OnPropertyChanged(nameof(CanSendTradeOffer));
            }
        }

        public void RemoveDestinationItem(Item item)
        {
            if (item != null)
            {
                SelectedDestinationItems.Remove(item);
                DestinationUserItems.Add(item);
                OnPropertyChanged(nameof(CanSendTradeOffer));
            }
        }

        public async Task CreateTradeOffer()
        {
            if (!CanSendTradeOffer) return;

            try
            {
                var trade = new ItemTrade(CurrentUser, SelectedUser, SelectedGame, TradeDescription);
                
                foreach (var item in SelectedSourceItems)
                {
                    trade.AddSourceUserItem(item);
                }

                foreach (var item in SelectedDestinationItems)
                {
                    trade.AddDestinationUserItem(item);
                }

                _dbConnector.CreateItemTrade(trade);
                await LoadActiveTradesAsync();

                // Clear selections
                SelectedSourceItems.Clear();
                SelectedDestinationItems.Clear();
                TradeDescription = string.Empty;
                LoadUserInventory();
                LoadDestinationUserInventory();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating trade offer: {ex.Message}");
            }
        }

        public async Task LoadActiveTradesAsync()
        {
            if (CurrentUser == null) return;

            try
            {
                var trades = _dbConnector.GetActiveItemTrades(CurrentUser.UserId);
                ActiveTrades.Clear();
                foreach (var trade in trades)
                {
                    ActiveTrades.Add(trade);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading active trades: {ex.Message}");
            }
        }

        public void LoadActiveTrades()
        {
            if (CurrentUser == null) return;

            try
            {
                var trades = _dbConnector.GetActiveItemTrades(CurrentUser.UserId);
                ActiveTrades.Clear();
                foreach (var trade in trades)
                {
                    ActiveTrades.Add(trade);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading active trades: {ex.Message}");
            }
        }

        public void LoadTradeHistory()
        {
            if (CurrentUser == null) return;

            try
            {
                var trades = _dbConnector.GetItemTradeHistory(CurrentUser.UserId);
                TradeHistory.Clear();
                foreach (var trade in trades)
                {
                    TradeHistory.Add(trade);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading trade history: {ex.Message}");
            }
        }

        public void AcceptTrade(ItemTrade trade)
        {
            try
            {
                if (trade.SourceUser.UserId == CurrentUser.UserId)
                {
                    //trade.AcceptedBySourceUser = true;
                    trade.AcceptBySourceUser();
                }
                else if (trade.DestinationUser.UserId == CurrentUser.UserId)
                {
                    trade.AcceptByDestinationUser();
                }

                // If both users have accepted, mark the trade as completed
                if (trade.AcceptedBySourceUser && trade.AcceptedByDestinationUser)
                {
  
                    trade.Complete();

                }

                _dbConnector.UpdateItemTrade(trade);

                // Refresh the trades lists and user inventories
                LoadActiveTrades();
                LoadTradeHistory();
                LoadUserInventory();
                LoadDestinationUserInventory();
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

        public async Task<bool> DeclineTrade(ItemTrade trade)
        {
            if (trade == null) return false;

            try
            {
                trade.Decline();
                _dbConnector.UpdateItemTrade(trade);
                await LoadActiveTradesAsync();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error declining trade: {ex.Message}");
                return false;
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public async Task LoadUsersAsync()
        {
            try
            {
                var users = _dbConnector.GetAllUsers();
                Users.Clear();
                foreach (var user in users)
                {
                    Users.Add(user);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading users: {ex.Message}");
            }
        }

        public async Task LoadGamesAsync()
        {
            try
            {
                var games = _dbConnector.GetGames();
                Games.Clear();
                foreach (var game in games)
                {
                    Games.Add(game);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading games: {ex.Message}");
            }
        }
    }
} 