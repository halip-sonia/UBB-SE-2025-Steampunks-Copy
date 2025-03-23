using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Steampunks.Views;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Steampunks
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            // Set default page
            NavView.SelectedItem = NavView.MenuItems[0];
            ContentFrame.Navigate(typeof(MarketplacePage));
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItemContainer != null)
            {
                string tag = args.SelectedItemContainer.Tag.ToString() ?? string.Empty;
                Type? pageType = tag switch
                {
                    "marketplace" => typeof(MarketplacePage),
                    "inventory" => typeof(InventoryPage),
                    "trading" => typeof(TradingPage),
                    _ => null
                };

                if (pageType != null)
                {
                    ContentFrame.Navigate(pageType);
                }
            }
        }
    }
}
