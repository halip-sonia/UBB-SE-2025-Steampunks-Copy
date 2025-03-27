using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Steampunks.ViewModels;
using Steampunks.Services;
using Steampunks.Repository.Inventory;
using Steampunks.Repository.GameRepo;
using Steampunks.Domain.Entities;

namespace Steampunks.Views
{
    public sealed partial class InventoryPage : Page
    {
        public InventoryViewModel ViewModel { get; }

        public InventoryPage()
        {
            InitializeComponent();
            
            // Create and set the ViewModel with proper dependencies
            var inventoryService = new InventoryService(
                new InventoryRepository(),
                new User("DefaultUser") // TODO: Replace with actual user from authentication
            );
            ViewModel = new InventoryViewModel(inventoryService);
            this.DataContext = ViewModel;
        }

        private void CreateTradeOffer_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            Frame.Navigate(typeof(TradingPage));
        }
    }
} 