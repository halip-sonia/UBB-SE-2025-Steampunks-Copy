using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Steampunks.ViewModels;
using Steampunks.Services;
using Steampunks.Repository.Inventory;
using Steampunks.Repository.GameRepo;
using Steampunks.Domain.Entities;
using Steampunks.DataLink;
using System.Diagnostics;
using System.Linq;

namespace Steampunks.Views
{
    public sealed partial class InventoryPage : Page
    {
        public InventoryViewModel ViewModel { get; }

        public InventoryPage()
        {
            try
            {
                Debug.WriteLine("Starting InventoryPage initialization...");
                this.InitializeComponent();
                Debug.WriteLine("XAML initialization completed.");

                Debug.WriteLine("Creating DatabaseConnector...");
                var dbConnector = new DatabaseConnector();
                Debug.WriteLine("DatabaseConnector created successfully.");

                Debug.WriteLine("Creating InventoryViewModel...");
                ViewModel = new InventoryViewModel(dbConnector);
                Debug.WriteLine("InventoryViewModel created successfully.");

                Debug.WriteLine("Setting DataContext...");
                this.DataContext = this;
                Debug.WriteLine("DataContext set successfully.");

                // Subscribe to user selection changes
                UserComboBox.SelectionChanged += UserComboBox_SelectionChanged;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in InventoryPage constructor: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    Debug.WriteLine($"Inner exception stack trace: {ex.InnerException.StackTrace}");
                }
                
                // Create a basic ViewModel with empty collections
                try
                {
                    ViewModel = new InventoryViewModel(new DatabaseConnector());
                    this.DataContext = this;
                }
                catch (Exception fallbackEx)
                {
                    Debug.WriteLine($"Error in fallback initialization: {fallbackEx.Message}");
                    Debug.WriteLine($"Fallback stack trace: {fallbackEx.StackTrace}");
                }
            }
        }

        private void UserComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (ViewModel.SelectedUser != null)
                {
                    Debug.WriteLine($"User selected: {ViewModel.SelectedUser.Username}");
                    ViewModel.LoadInventoryItems();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in UserComboBox_SelectionChanged: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        private void CreateTradeOffer_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            try
            {
                Frame.Navigate(typeof(TradingPage));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error navigating to TradingPage: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                if (e.ClickedItem is Item selectedItem)
                {
                    ViewModel.SelectedItem = selectedItem;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in ListView_ItemClick: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        private void ItemImage_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            try
            {
                if (sender is Image itemImage && itemImage.Parent is Grid grid)
                {
                    var defaultImage = grid.Children.OfType<Image>().FirstOrDefault(img => img.Name == "DefaultImage");
                    if (defaultImage != null)
                    {
                        itemImage.Visibility = Visibility.Collapsed;
                        defaultImage.Visibility = Visibility.Visible;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error handling image failure: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
    }
} 