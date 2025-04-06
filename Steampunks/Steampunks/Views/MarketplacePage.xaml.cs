// <copyright file="MarketplacePage.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Steampunks.Views
{
    using System;
    using Microsoft.UI.Xaml.Controls;
    using Steampunks.Domain.Entities;
    using Steampunks.Services;
    using Steampunks.Services.MarketplaceService;
    using Steampunks.ViewModels;
    using Windows.Foundation;

    /// <summary>
    /// Represents the page that displays items available in the marketplace.
    /// </summary>
    public sealed partial class MarketplacePage : Page
    {
        private const string ErrorDialogTitle = "Error";
        private const string SelectUserErrorMessage = "Please select a user before buying items.";
        private const string OkButtonText = "OK";
        private const string SuccessDialogTitle = "Success";
        private const string ItemPurchasedMessage = "Item purchased successfully!";
        private const string UnexpectedErrorMessage = "An unexpected error occurred. Please try again.";

        /// <summary>
        /// Initializes a new instance of the <see cref="MarketplacePage"/> class.
        /// </summary>
        public MarketplacePage()
        {
            this.InitializeComponent();

            var marketplaceServiceInstance = new MarketplaceService(new Repository.Marketplace.MarketplaceRepository());
            this.DataContext = new MarketplaceViewModel(marketplaceServiceInstance);
        }

        /// <summary>
        /// Handles item click events in the GridView, opening a dialog and processing item purchase.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="eventArgs">The item click event arguments.</param>
        private void OnMarketplaceGridViewItemClicked(object sender, ItemClickEventArgs eventArgs)
        {
            if (eventArgs.ClickedItem is Item clickedMarketplaceItem)
            {
                var marketplaceViewModel = (MarketplaceViewModel)this.DataContext;
                marketplaceViewModel.SelectedItem = clickedMarketplaceItem;

                this.ItemDetailsDialog.XamlRoot = this.XamlRoot;

                this.ItemDetailsDialog.ShowAsync().Completed = async (asyncOperation, asyncStatus) =>
                {
                    var dialogResult = (ContentDialogResult)asyncOperation.GetResults();
                    if (dialogResult == ContentDialogResult.Secondary)
                    {
                        try
                        {
                            if (marketplaceViewModel.CurrentUser == null)
                            {
                                var missingUserDialog = new ContentDialog
                                {
                                    Title = ErrorDialogTitle,
                                    Content = SelectUserErrorMessage,
                                    CloseButtonText = OkButtonText,
                                    XamlRoot = this.XamlRoot,
                                };
                                await missingUserDialog.ShowAsync();
                                return;
                            }

                            bool purchaseSuccess = await marketplaceViewModel.BuyItemAsync();
                            if (purchaseSuccess)
                            {
                                var purchaseSuccessDialog = new ContentDialog
                                {
                                    Title = SuccessDialogTitle,
                                    Content = ItemPurchasedMessage,
                                    CloseButtonText = OkButtonText,
                                    XamlRoot = this.XamlRoot,
                                };
                                await purchaseSuccessDialog.ShowAsync();
                            }
                        }
                        catch (InvalidOperationException operationException)
                        {
                            var operationErrorDialog = new ContentDialog
                            {
                                Title = ErrorDialogTitle,
                                Content = operationException.Message,
                                CloseButtonText = OkButtonText,
                                XamlRoot = this.XamlRoot,
                            };
                            await operationErrorDialog.ShowAsync();
                        }
                        catch (Exception generalException)
                        {
                            var generalErrorDialog = new ContentDialog
                            {
                                Title = ErrorDialogTitle,
                                Content = UnexpectedErrorMessage,
                                CloseButtonText = OkButtonText,
                                XamlRoot = this.XamlRoot,
                            };
                            await generalErrorDialog.ShowAsync();

                            System.Diagnostics.Debug.WriteLine($"Error in OnMarketplaceGridViewItemClicked: {generalException.Message}");
                            System.Diagnostics.Debug.WriteLine($"Stack trace: {generalException.StackTrace}");
                        }
                    }
                };
            }
        }
    }
}