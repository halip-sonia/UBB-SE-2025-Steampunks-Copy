using Microsoft.UI.Xaml.Controls;
using Steampunks.ViewModels;
using Steampunks.Services;
using Steampunks.Domain.Entities;

namespace Steampunks.Views
{
    public sealed partial class MarketplacePage : Page
    {
        public MarketplacePage()
        {
            this.InitializeComponent();
            
            // Create and set the ViewModel
            var marketplaceService = new MarketplaceService(new Repository.Marketplace.MarketplaceRepository(), new User("DefaultUser"));
            this.DataContext = new MarketplaceViewModel(marketplaceService);
        }
    }
}