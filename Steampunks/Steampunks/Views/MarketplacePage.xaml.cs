using Microsoft.UI.Xaml.Controls;
using Steampunks.ViewModels;
using Steampunks.Services;
using Steampunks.Domain.Entities;
using Windows.Foundation;
using System;
using Steampunks.Services.MarketplaceService;

namespace Steampunks.Views
{
    public sealed partial class MarketplacePage : Page
    {
        public MarketplacePage()
        {
            this.InitializeComponent();
            
            // Create and set the ViewModel
            var marketplaceService = new MarketplaceService(new Repository.Marketplace.MarketplaceRepository());
            this.DataContext = new MarketplaceViewModel(marketplaceService);
        }

        private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is Item selectedItem)
            {
                var viewModel = (MarketplaceViewModel)DataContext;
                viewModel.SelectedItem = selectedItem;

                // Set XamlRoot for the item details dialog
                ItemDetailsDialog.XamlRoot = this.XamlRoot;

                ItemDetailsDialog.ShowAsync().Completed = async (asyncInfo, status) =>
                {
                    var result = (ContentDialogResult)asyncInfo.GetResults();
                    if (result == ContentDialogResult.Secondary)
                    {
                        try
                        {
                            // Check if a user is selected
                            if (viewModel.CurrentUser == null)
                            {
                                var errorDialog = new ContentDialog
                                {
                                    Title = "Error",
                                    Content = "Please select a user before buying items.",
                                    CloseButtonText = "OK",
                                    XamlRoot = this.XamlRoot
                                };
                                await errorDialog.ShowAsync();
                                return;
                            }

                            // Buy button was clicked
                            bool success = await viewModel.BuyItemAsync();
                            if (success)
                            {
                                // Show success message
                                var successDialog = new ContentDialog
                                {
                                    Title = "Success",
                                    Content = "Item purchased successfully!",
                                    CloseButtonText = "OK",
                                    XamlRoot = this.XamlRoot
                                };
                                await successDialog.ShowAsync();
                            }
                        }
                        catch (InvalidOperationException ex)
                        {
                            // Show specific error message
                            var errorDialog = new ContentDialog
                            {
                                Title = "Error",
                                Content = ex.Message,
                                CloseButtonText = "OK",
                                XamlRoot = this.XamlRoot
                            };
                            await errorDialog.ShowAsync();
                        }
                        catch (Exception ex)
                        {
                            // Show generic error message
                            var errorDialog = new ContentDialog
                            {
                                Title = "Error",
                                Content = "An unexpected error occurred. Please try again.",
                                CloseButtonText = "OK",
                                XamlRoot = this.XamlRoot
                            };
                            await errorDialog.ShowAsync();
                            
                            // Log the error
                            System.Diagnostics.Debug.WriteLine($"Error in GridView_ItemClick: {ex.Message}");
                            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                        }
                    }
                };
            }
        }
    }
}