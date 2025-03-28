using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Steampunks.Domain.Entities;
using Steampunks.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Steampunks.Views
{
    public sealed partial class TradeView : Page
    {
        public TradeViewModel ViewModel { get; }

        public TradeView()
        {
            this.InitializeComponent();
            ViewModel = App.GetService<TradeViewModel>();
            DataContext = this;
        }

        private async void TradeView_Loaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.LoadUsersAsync();
            await ViewModel.LoadGamesAsync();
        }

        private void AddSourceItem_Click(object sender, RoutedEventArgs e)
        {
            if (SourceItemsListView.SelectedItems.Count > 0)
            {
                foreach (var item in SourceItemsListView.SelectedItems)
                {
                    if (item is Item selectedItem)
                    {
                        ViewModel.AddSourceItem(selectedItem);
                    }
                }
                SourceItemsListView.SelectedItems.Clear();
            }
        }

        private void AddDestinationItem_Click(object sender, RoutedEventArgs e)
        {
            if (DestinationItemsListView.SelectedItems.Count > 0)
            {
                foreach (var item in DestinationItemsListView.SelectedItems)
                {
                    if (item is Item selectedItem)
                    {
                        ViewModel.AddDestinationItem(selectedItem);
                    }
                }
                DestinationItemsListView.SelectedItems.Clear();
            }
        }

        private void RemoveSourceItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Item item)
            {
                ViewModel.RemoveSourceItem(item);
            }
        }

        private void RemoveDestinationItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Item item)
            {
                ViewModel.RemoveDestinationItem(item);
            }
        }

        private async void SendTradeOffer_Click(object sender, RoutedEventArgs e)
        {
            if (!ViewModel.CanSendTradeOffer)
            {
                var dialog = new ContentDialog
                {
                    Title = "Cannot Send Trade",
                    Content = "Please select a user to trade with, add items to trade, and provide a trade description.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };

                await dialog.ShowAsync();
                return;
            }

            var confirmDialog = new ContentDialog
            {
                Title = "Confirm Trade",
                Content = "Are you sure you want to send this trade offer?",
                PrimaryButtonText = "Send",
                CloseButtonText = "Cancel",
                XamlRoot = this.XamlRoot
            };

            var result = await confirmDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                await ViewModel.CreateTradeOffer();
            }
        }

        private async void AcceptTrade_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is ItemTrade trade)
            {
                var confirmDialog = new ContentDialog
                {
                    Title = "Accept Trade",
                    Content = "Are you sure you want to accept this trade?",
                    PrimaryButtonText = "Accept",
                    CloseButtonText = "Cancel",
                    XamlRoot = this.XamlRoot
                };

                var result = await confirmDialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                   ViewModel.AcceptTrade(trade);
                }
            }
        }

        private async void DeclineTrade_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is ItemTrade trade)
            {
                var confirmDialog = new ContentDialog
                {
                    Title = "Decline Trade",
                    Content = "Are you sure you want to decline this trade?",
                    PrimaryButtonText = "Decline",
                    CloseButtonText = "Cancel",
                    XamlRoot = this.XamlRoot
                };

                var result = await confirmDialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    await ViewModel.DeclineTrade(trade);
                }
            }
        }
    }
} 