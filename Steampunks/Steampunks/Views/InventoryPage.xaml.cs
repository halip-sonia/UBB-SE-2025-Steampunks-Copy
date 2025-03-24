using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Steampunks.Views
{
    public sealed partial class InventoryPage : Page
    {
        public InventoryPage()
        {
            InitializeComponent();
        }

        private void CreateTradeOffer_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            Frame.Navigate(typeof(TradingPage));
        }
    }
} 