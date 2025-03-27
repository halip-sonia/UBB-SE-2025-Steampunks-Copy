using Microsoft.UI.Xaml.Controls;
using Steampunks.ViewModels;
using Steampunks.Services;
using Steampunks.Domain.Entities;
using Windows.Foundation;
using System;

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
                ItemDetailsDialog.ShowAsync().Completed = async (asyncInfo, status) =>
                {
                    var result = (ContentDialogResult)asyncInfo.GetResults();
                    if (result == ContentDialogResult.Secondary)
                    {
                        try
                        {
                            // Buy button was clicked
                            bool success = await viewModel.BuyItemAsync();
                            if (success)
                            {
                                // Show success message
                                var successDialog = new ContentDialog
                                {
                                    Title = "Success",
                                    Content = "Item purchased successfully!",
                                    CloseButtonText = "OK"
                                };
                                successDialog.ShowAsync();
                            }
                        }
                        catch (Exception ex)
                        {
                            // Show error message
                            var errorDialog = new ContentDialog
                            {
                                Title = "Error",
                                Content = ex.Message,
                                CloseButtonText = "OK"
                            };
                            errorDialog.ShowAsync();
                        }
                    }
                };
            }
        }
    }
}