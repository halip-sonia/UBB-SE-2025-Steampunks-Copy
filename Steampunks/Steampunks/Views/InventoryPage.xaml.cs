// <copyright file="InventoryPage.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Steampunks.Views
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Navigation;
    using Steampunks.DataLink;
    using Steampunks.Domain.Entities;
    using Steampunks.Repository.GameRepo;
    using Steampunks.Repository.Inventory;
    using Steampunks.Services;
    using Steampunks.ViewModels;

    /// <summary>
    /// Page responsible for displaying and managing a user's inventory.
    /// </summary>
    public sealed partial class InventoryPage : Page
    {
        private const string ConfirmSaleTitle = "Confirm Sale";
        private const string ConfirmSaleMessageFormat = "Are you sure you want to sell {0}?";
        private const string SuccessDialogTitle = "Success";
        private const string SuccessDialogMessageFormat = "{0} has been successfully listed for sale!";
        private const string ErrorDialogTitle = "Error";
        private const string ErrorDialogMessage = "Failed to sell the item. Please try again.";
        private const string OkButtonText = "OK";
        private const string YesButtonText = "Yes";
        private const string NoButtonText = "No";

        /// <summary>
        /// Initializes a new instance of the <see cref="InventoryPage"/> class.
        /// </summary>
        public InventoryPage()
        {
            try
            {
                Debug.WriteLine("Starting InventoryPage initialization...");
                this.InitializeComponent();
                Debug.WriteLine("XAML initialization completed.");

                Debug.WriteLine("Creating DatabaseConnector...");
                var databaseConnector = new DatabaseConnector();
                Debug.WriteLine("DatabaseConnector created successfully.");

                Debug.WriteLine("Creating InventoryViewModel...");
                this.ViewModel = new InventoryViewModel(databaseConnector);
                Debug.WriteLine("InventoryViewModel created successfully.");

                Debug.WriteLine("Setting DataContext...");
                this.DataContext = this;
                Debug.WriteLine("DataContext set successfully.");

                // Subscribe to user selection changes
                this.UserComboBox.SelectionChanged += this.OnUserSelectionChanged;
            }
            catch (Exception exception)
            {
                Debug.WriteLine($"Error in InventoryPage constructor: {exception.Message}");
                Debug.WriteLine($"Stack trace: {exception.StackTrace}");
                if (exception.InnerException != null)
                {
                    Debug.WriteLine($"Inner exception: {exception.InnerException.Message}");
                    Debug.WriteLine($"Inner exception stack trace: {exception.InnerException.StackTrace}");
                }

                // Create a basic ViewModel with empty collections
                try
                {
                    this.ViewModel = new InventoryViewModel(new DatabaseConnector());
                    this.DataContext = this;
                }
                catch (Exception fallbackException)
                {
                    Debug.WriteLine($"Error in fallback initialization: {fallbackException.Message}");
                    Debug.WriteLine($"Fallback stack trace: {fallbackException.StackTrace}");
                }
            }
        }

        /// <summary>
        /// Gets the view model for this page.
        /// </summary>
        public InventoryViewModel? ViewModel { get; private set; }

        private async void OnUserSelectionChanged(object sender, SelectionChangedEventArgs eventArgs)
        {
            try
            {
                var user = this.ViewModel?.SelectedUser;
                if (user != null)
                {
                    Debug.WriteLine($"User selected: {user.Username}");
                    await this.ViewModel!.LoadInventoryItemsAsync();
                }
            }
            catch (Exception exception)
            {
                Debug.WriteLine($"Error in OnUserSelectionChanged: {exception.Message}");
                Debug.WriteLine($"Stack trace: {exception.StackTrace}");
            }
        }

        /// <summary>
        /// Navigates to the trading page when the "Create Trade Offer" button is clicked.
        /// </summary>
        private void OnCreateTradeOfferButtonClicked(object sender, Microsoft.UI.Xaml.RoutedEventArgs eventArgs)
        {
            try
            {
                this.Frame.Navigate(typeof(TradingPage));
            }
            catch (Exception exception)
            {
                Debug.WriteLine($"Error navigating to TradingPage: {exception.Message}");
                Debug.WriteLine($"Stack trace: {exception.StackTrace}");
            }
        }

        /// <summary>
        /// Triggered when an item is clicked in the list. Updates the selected item in the ViewModel.
        /// </summary>
        private void OnInventoryItemClicked(object sender, ItemClickEventArgs eventArgs)
        {
            try
            {
                if (this.ViewModel != null && eventArgs.ClickedItem is Item selectedItem)
                {
                    this.ViewModel.SelectedItem = selectedItem;
                }
            }
            catch (Exception exception)
            {
                Debug.WriteLine($"Error in OnInventoryItemClicked: {exception.Message}");
                Debug.WriteLine($"Stack trace: {exception.StackTrace}");
            }
        }

        /// <summary>
        /// Replaces a failed image load with a default image.
        /// </summary>
        private void OnItemImageLoadFailed(object sender, ExceptionRoutedEventArgs eventArgs)
        {
            try
            {
                if (sender is Image itemImage && itemImage.Parent is Grid parentGrid)
                {
                    var defaultImage = parentGrid.Children.OfType<Image>().FirstOrDefault(image => image.Name == "DefaultImage");
                    if (defaultImage != null)
                    {
                        itemImage.Visibility = Visibility.Collapsed;
                        defaultImage.Visibility = Visibility.Visible;
                    }
                }
            }
            catch (Exception exception)
            {
                Debug.WriteLine($"Error handling image failure: {exception.Message}");
                Debug.WriteLine($"Stack trace: {exception.StackTrace}");
            }
        }

        /// <summary>
        /// Handles the confirmation dialog and attempts to list the selected item for sale.
        /// </summary>
        /// <param name="sender">The button that was clicked to trigger the sale action.</param>
        /// <param name="eventArgs">The routed event arguments associated with the button click.</param>
        private async void OnSellItemButtonClicked(object sender, RoutedEventArgs eventArgs)
        {
            Button? clickedSellButton = sender as Button;
            Item? selectedItem = clickedSellButton?.DataContext as Item;

            if (selectedItem == null)
            {
                return;
            }

            ContentDialog confirmationDialog = new ContentDialog
            {
                XamlRoot = this.XamlRoot,
                Title = ConfirmSaleTitle,
                Content = string.Format(ConfirmSaleMessageFormat, selectedItem.ItemName),
                PrimaryButtonText = YesButtonText,
                CloseButtonText = NoButtonText,
                DefaultButton = ContentDialogButton.Close,
            };

            ContentDialogResult userResponse = await confirmationDialog.ShowAsync();

            if (this.ViewModel != null && userResponse == ContentDialogResult.Primary)
            {
                bool wasItemListed = await this.ViewModel.SellItemAsync(selectedItem);
                if (wasItemListed)
                {
                    var successDialog = new ContentDialog
                    {
                        XamlRoot = this.XamlRoot,
                        Title = SuccessDialogTitle,
                        Content = string.Format(SuccessDialogMessageFormat, selectedItem.ItemName),
                        CloseButtonText = OkButtonText,
                    };
                    await successDialog.ShowAsync();
                }
                else
                {
                    var errorDialog = new ContentDialog
                    {
                        XamlRoot = this.XamlRoot,
                        Title = ErrorDialogTitle,
                        Content = ErrorDialogMessage,
                        CloseButtonText = OkButtonText,
                    };
                    await errorDialog.ShowAsync();
                }
            }
        }
    }
}