using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Steampunks.Domain.Entities;
using Steampunks.DataLink;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;
using Steampunks.ViewModels;
using Steampunks.Services;

namespace Steampunks.Views
{
    public sealed partial class TradingPage : Page
    {
        private readonly DatabaseConnector _dbConnector;
        private readonly TradeService _tradeService;
        private readonly UserService _userService;
        private readonly GameService _gameService;
        private TradeViewModel ViewModel { get; set; }
        private ObservableCollection<ItemTrade> ActiveTrades { get; set; }
        private ObservableCollection<TradeHistoryViewModel> TradeHistory { get; set; }
        private User _currentUser;
        private User _recipientUser;
        private ObservableCollection<Item> _sourceItems;
        private ObservableCollection<Item> _destinationItems;
        private ObservableCollection<Item> _selectedSourceItems;
        private ObservableCollection<Item> _selectedDestinationItems;

        public TradingPage()
        {
            InitializeComponent();
            _dbConnector = new DatabaseConnector();
            _tradeService = new TradeService(_dbConnector);
            _userService = new UserService(_dbConnector);
            _gameService = new GameService(_dbConnector);
            ViewModel = new TradeViewModel(_tradeService, _userService, _gameService, _dbConnector);
            ActiveTrades = new ObservableCollection<ItemTrade>();
            TradeHistory = new ObservableCollection<TradeHistoryViewModel>();
            _sourceItems = new ObservableCollection<Item>();
            _destinationItems = new ObservableCollection<Item>();
            _selectedSourceItems = new ObservableCollection<Item>();
            _selectedDestinationItems = new ObservableCollection<Item>();
            
            // Set up ListViews
            SourceItemsListView.ItemsSource = _sourceItems;
            DestinationItemsListView.ItemsSource = _destinationItems;
            SelectedSourceItemsListView.ItemsSource = _selectedSourceItems;
            SelectedDestinationItemsListView.ItemsSource = _selectedDestinationItems;
            
            LoadUsers();
            LoadGames();
            LoadActiveTrades();
            LoadTradeHistory();
        }

        private void LoadUsers()
        {
            try
            {
                var users = _dbConnector.GetAllUsers();
                UserComboBox.ItemsSource = users;
                UserComboBox.DisplayMemberPath = "Username";

                // Set the current user as the selected user
                _currentUser = _dbConnector.GetCurrentUser();
                UserComboBox.SelectedItem = users.FirstOrDefault(u => u.UserId == _currentUser.UserId);

                LoadActiveTrades();
                LoadTradeHistory();
            }
            catch (Exception ex)
            {
                ErrorMessage.Text = "Error loading users. Please try again later.";
                System.Diagnostics.Debug.WriteLine($"Error loading users: {ex.Message}");
            }
        }

        private void UserComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _currentUser = UserComboBox.SelectedItem as User;
            if (_currentUser != null)
            {
                // Update available users for trading (exclude current user)
                var users = _dbConnector.GetAllUsers().Where(u => u.UserId != _currentUser.UserId).ToList();
                RecipientComboBox.ItemsSource = users;
                RecipientComboBox.DisplayMemberPath = "Username";

                // Load current user's items
                LoadUserItems();
            }
        }

        private void RecipientComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _recipientUser = RecipientComboBox.SelectedItem as User;
            if (_recipientUser != null)
            {
                // Load recipient's items
                LoadRecipientItems();
            }
        }

        private void LoadUserItems()
        {
            try
            {
                _sourceItems.Clear();
                _selectedSourceItems.Clear();

                if (_currentUser != null)
                {
                    var items = _dbConnector.GetUserItems(_currentUser.UserId);
                    var selectedGame = GameComboBox.SelectedItem as Game;

                    if (selectedGame != null)
                    {
                        items = items.Where(i => i.GetCorrespondingGame().GameId == selectedGame.GameId).ToList();
                    }

                    foreach (var item in items)
                    {
                        _sourceItems.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage.Text = "Error loading your items. Please try again later.";
                System.Diagnostics.Debug.WriteLine($"Error loading user items: {ex.Message}");
            }
        }

        private void LoadRecipientItems()
        {
            try
            {
                _destinationItems.Clear();
                _selectedDestinationItems.Clear();

                if (_recipientUser != null)
                {
                    var items = _dbConnector.GetUserItems(_recipientUser.UserId);
                    var selectedGame = GameComboBox.SelectedItem as Game;

                    if (selectedGame != null)
                    {
                        items = items.Where(i => i.GetCorrespondingGame().GameId == selectedGame.GameId).ToList();
                    }

                    foreach (var item in items)
                    {
                        _destinationItems.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage.Text = "Error loading recipient's items. Please try again later.";
                System.Diagnostics.Debug.WriteLine($"Error loading recipient items: {ex.Message}");
            }
        }

        private void LoadGames()
        {
            try
            {
                var games = _dbConnector.GetAllGames();
                GameComboBox.ItemsSource = games;
                GameComboBox.DisplayMemberPath = "Title";
                System.Diagnostics.Debug.WriteLine($"Successfully loaded {games.Count} games");
            }
            catch (Exception ex)
            {
                var errorMessage = $"Error loading games: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $"\nInner error: {ex.InnerException.Message}";
                }
                ErrorMessage.Text = errorMessage;
                System.Diagnostics.Debug.WriteLine(errorMessage);
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        private void LoadActiveTrades()
        {
            try
            {
                var trades = _dbConnector.GetActiveItemTrades(_currentUser.UserId);
                ActiveTrades.Clear();

                foreach (var trade in trades)
                {
                    ActiveTrades.Add(trade);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage.Text = "Error loading active trades. Please try again later.";
                System.Diagnostics.Debug.WriteLine($"Error loading active trades: {ex.Message}");
            }
        }

        private void LoadTradeHistory()
        {
            try
            {
                var trades = _dbConnector.GetItemTradeHistory(_currentUser.UserId);
                TradeHistory.Clear();

                foreach (var trade in trades)
                {
                    var isSourceUser = trade.SourceUser.UserId == _currentUser.UserId;
                    var partner = isSourceUser ? trade.DestinationUser : trade.SourceUser;
                    
                    var statusColor = trade.TradeStatus == "Completed" 
                        ? new SolidColorBrush(Colors.Green) 
                        : new SolidColorBrush(Colors.Red);

                    TradeHistory.Add(new TradeHistoryViewModel
                    {
                        TradeId = trade.TradeId,
                        PartnerName = partner.Username,
                        TradeItems = trade.SourceUserItems.Concat(trade.DestinationUserItems).ToList(),
                        TradeDescription = trade.TradeDescription,
                        TradeStatus = trade.TradeStatus,
                        TradeDate = trade.TradeDate.ToString("MMM dd, yyyy HH:mm"),
                        StatusColor = statusColor,
                        IsSourceUser = isSourceUser
                    });
                }
            }
            catch (Exception ex)
            {
                ErrorMessage.Text = "Error loading trade history. Please try again later.";
                System.Diagnostics.Debug.WriteLine($"Error loading trade history: {ex.Message}");
            }
        }

        private void GameComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadUserItems();
            LoadRecipientItems();
        }

        private void AddSourceItem_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = SourceItemsListView.SelectedItems.Cast<Item>().ToList();
            foreach (var item in selectedItems)
            {
                if (!_selectedSourceItems.Contains(item))
                {
                    _selectedSourceItems.Add(item);
                    _sourceItems.Remove(item);
                }
            }
        }

        private void AddDestinationItem_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = DestinationItemsListView.SelectedItems.Cast<Item>().ToList();
            foreach (var item in selectedItems)
            {
                if (!_selectedDestinationItems.Contains(item))
                {
                    _selectedDestinationItems.Add(item);
                    _destinationItems.Remove(item);
                }
            }
        }

        private void RemoveSourceItem_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var item = button.Tag as Item;
            if (item != null)
            {
                _selectedSourceItems.Remove(item);
                _sourceItems.Add(item);
            }
        }

        private void RemoveDestinationItem_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var item = button.Tag as Item;
            if (item != null)
            {
                _selectedDestinationItems.Remove(item);
                _destinationItems.Add(item);
            }
        }

        private void CreateTradeOffer_Click(object sender, RoutedEventArgs e)
        {
            ErrorMessage.Text = string.Empty;
            SuccessMessage.Text = string.Empty;

            if (_currentUser == null)
            {
                ErrorMessage.Text = "Please select your user.";
                return;
            }

            if (_recipientUser == null)
            {
                ErrorMessage.Text = "Please select a user to trade with.";
                return;
            }

            if (!_selectedSourceItems.Any() && !_selectedDestinationItems.Any())
            {
                ErrorMessage.Text = "Please select at least one item to trade.";
                return;
            }

            string description = DescriptionTextBox.Text;
            if (string.IsNullOrWhiteSpace(description))
            {
                ErrorMessage.Text = "Please enter a trade description.";
                return;
            }

            try
            {
                // Get the game from the first selected item (if any)
                var game = _selectedSourceItems.FirstOrDefault()?.Game ?? 
                          _selectedDestinationItems.FirstOrDefault()?.Game;

                if (game == null)
                {
                    ErrorMessage.Text = "Unable to determine the game for the trade.";
                    return;
                }

                var itemTrade = new ItemTrade(_currentUser, _recipientUser, game, description);

                // Add selected items to the trade
                foreach (var item in _selectedSourceItems)
                {
                    itemTrade.AddSourceUserItem(item);
                }

                foreach (var item in _selectedDestinationItems)
                {
                    itemTrade.AddDestinationUserItem(item);
                }

                _dbConnector.CreateItemTrade(itemTrade);

                // Clear form
                GameComboBox.SelectedIndex = -1;
                RecipientComboBox.SelectedIndex = -1;
                DescriptionTextBox.Text = string.Empty;
                _selectedSourceItems.Clear();
                _selectedDestinationItems.Clear();
                LoadUserItems();
                LoadRecipientItems();

                // Show success message and refresh trades
                SuccessMessage.Text = "Trade offer created successfully!";
                LoadActiveTrades();
                LoadTradeHistory();
            }
            catch (Exception ex)
            {
                ErrorMessage.Text = $"An error occurred while creating the trade offer: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Error creating item trade: {ex.Message}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
            }
        }

        private void ActiveTradesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListView listView && listView.SelectedItem is ItemTrade selectedTrade)
            {
                ViewModel.SelectedTrade = selectedTrade;
            }
        }

        private async void AcceptTrade_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedTrade == null) return;

            try
            {
                ViewModel.AcceptTrade(ViewModel.SelectedTrade);
                ViewModel.SelectedTrade = null;
                LoadActiveTrades();
                LoadTradeHistory();
            }
            catch (Exception ex)
            {
                ErrorMessage.Text = $"Error accepting trade: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Error accepting trade: {ex.Message}");
            }
        }

        private async void DeclineTrade_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedTrade == null) return;

            try
            {
                await ViewModel.DeclineTrade(ViewModel.SelectedTrade);
                ViewModel.SelectedTrade = null;
                LoadActiveTrades();
                LoadTradeHistory();
            }
            catch (Exception ex)
            {
                ErrorMessage.Text = $"Error declining trade: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Error declining trade: {ex.Message}");
            }
        }

        private class TradeHistoryViewModel
        {
            public int TradeId { get; set; }
            public string PartnerName { get; set; }
            public List<Item> TradeItems { get; set; }
            public string TradeDescription { get; set; }
            public string TradeStatus { get; set; }
            public string TradeDate { get; set; }
            public SolidColorBrush StatusColor { get; set; }
            public bool IsSourceUser { get; set; }
        }
    }
} 