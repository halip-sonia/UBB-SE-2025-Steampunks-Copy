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

namespace Steampunks.Views
{
    public sealed partial class TradingPage : Page
    {
        private readonly DatabaseConnector _dbConnector;
        private ObservableCollection<TradeViewModel> ActiveTrades { get; set; }
        private ObservableCollection<TradeHistoryViewModel> TradeHistory { get; set; }

        public TradingPage()
        {
            InitializeComponent();
            _dbConnector = new DatabaseConnector();
            ActiveTrades = new ObservableCollection<TradeViewModel>();
            TradeHistory = new ObservableCollection<TradeHistoryViewModel>();
            ActiveTradesListView.ItemsSource = ActiveTrades;
            TradeHistoryListView.ItemsSource = TradeHistory;
            LoadGames();
            LoadActiveTrades();
            LoadTradeHistory();
        }

        private void LoadGames()
        {
            try
            {
                var games = _dbConnector.GetAllGames();
                GameComboBox.ItemsSource = games;
                GameComboBox.DisplayMemberPath = null; // Use ToString() instead
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
                var currentUser = _dbConnector.GetCurrentUser();
                var trades = _dbConnector.GetActiveGameTrades();
                ActiveTrades.Clear();

                foreach (var trade in trades)
                {
                    var isSourceUser = trade.GetSourceUser().UserId == currentUser.UserId;
                    var partner = isSourceUser ? trade.GetDestinationUser() : trade.GetSourceUser();
                    
                    ActiveTrades.Add(new TradeViewModel
                    {
                        TradeId = trade.TradeId,
                        PartnerName = partner.Username,
                        GameTitle = trade.GetTradeGame().Title,
                        TradeDescription = trade.TradeDescription,
                        IsSourceUser = isSourceUser
                    });
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
                var currentUser = _dbConnector.GetCurrentUser();
                var trades = _dbConnector.GetTradeHistory();
                TradeHistory.Clear();

                foreach (var trade in trades)
                {
                    var isSourceUser = trade.GetSourceUser().UserId == currentUser.UserId;
                    var partner = isSourceUser ? trade.GetDestinationUser() : trade.GetSourceUser();
                    
                    var statusColor = trade.TradeStatus == "Completed" 
                        ? new SolidColorBrush(Colors.Green) 
                        : new SolidColorBrush(Colors.Red);

                    TradeHistory.Add(new TradeHistoryViewModel
                    {
                        TradeId = trade.TradeId,
                        PartnerName = partner.Username,
                        GameTitle = trade.GetTradeGame().Title,
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

        private void CreateGameOffer_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            // Clear previous messages
            ErrorMessage.Text = string.Empty;
            SuccessMessage.Text = string.Empty;

            if (GameComboBox.SelectedItem is not Game selectedGame)
            {
                ErrorMessage.Text = "Please select a game for the trade.";
                return;
            }

            string recipientUsername = RecipientTextBox.Text;
            string description = DescriptionTextBox.Text;

            if (string.IsNullOrWhiteSpace(recipientUsername))
            {
                ErrorMessage.Text = "Please enter a recipient username.";
                return;
            }

            if (string.IsNullOrWhiteSpace(description))
            {
                ErrorMessage.Text = "Please enter a trade description.";
                return;
            }

            try
            {
                var currentUser = _dbConnector.GetCurrentUser();
                var recipientUser = _dbConnector.GetUserByUsername(recipientUsername);

                if (recipientUser == null)
                {
                    ErrorMessage.Text = "Recipient user not found.";
                    return;
                }

                if (currentUser.UserId == recipientUser.UserId)
                {
                    ErrorMessage.Text = "You cannot create a trade offer with yourself.";
                    return;
                }

                var gameTrade = new GameTrade(currentUser, recipientUser, selectedGame, description);
                _dbConnector.CreateGameTrade(gameTrade);

                // Clear form
                GameComboBox.SelectedIndex = -1;
                RecipientTextBox.Text = string.Empty;
                DescriptionTextBox.Text = string.Empty;

                // Show success message
                SuccessMessage.Text = "Trade offer created successfully!";
            }
            catch (Exception ex)
            {
                ErrorMessage.Text = "An error occurred while creating the trade offer. Please try again later.";
                System.Diagnostics.Debug.WriteLine($"Error creating game trade: {ex.Message}");
            }
        }

        private async void AcceptTrade_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            var button = (Button)sender;
            var trade = (TradeViewModel)button.DataContext;

            ContentDialog dialog = new ContentDialog
            {
                Title = "Confirm Trade Acceptance",
                Content = "Are you sure you want to accept this trade?",
                PrimaryButtonText = "Accept",
                CloseButtonText = "Cancel",
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                try
                {
                    _dbConnector.AcceptTrade(trade.TradeId);
                    LoadActiveTrades(); // Refresh active trades
                    LoadTradeHistory(); // Refresh trade history
                    SuccessMessage.Text = "Trade accepted successfully!";
                }
                catch (Exception ex)
                {
                    ErrorMessage.Text = "Error accepting trade. Please try again later.";
                    System.Diagnostics.Debug.WriteLine($"Error accepting trade: {ex.Message}");
                }
            }
        }

        private async void DeclineTrade_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            var button = (Button)sender;
            var trade = (TradeViewModel)button.DataContext;

            ContentDialog dialog = new ContentDialog
            {
                Title = "Confirm Trade Decline",
                Content = "Are you sure you want to decline this trade?",
                PrimaryButtonText = "Decline",
                CloseButtonText = "Cancel",
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                try
                {
                    _dbConnector.DeclineTrade(trade.TradeId);
                    LoadActiveTrades(); // Refresh active trades
                    LoadTradeHistory(); // Refresh trade history
                    SuccessMessage.Text = "Trade declined successfully!";
                }
                catch (Exception ex)
                {
                    ErrorMessage.Text = "Error declining trade. Please try again later.";
                    System.Diagnostics.Debug.WriteLine($"Error declining trade: {ex.Message}");
                }
            }
        }

        private class TradeViewModel
        {
            public int TradeId { get; set; }
            public string PartnerName { get; set; }
            public string GameTitle { get; set; }
            public string TradeDescription { get; set; }
            public bool IsSourceUser { get; set; }
        }

        private class TradeHistoryViewModel
        {
            public int TradeId { get; set; }
            public string PartnerName { get; set; }
            public string GameTitle { get; set; }
            public string TradeDescription { get; set; }
            public string TradeStatus { get; set; }
            public string TradeDate { get; set; }
            public SolidColorBrush StatusColor { get; set; }
            public bool IsSourceUser { get; set; }
        }
    }
} 