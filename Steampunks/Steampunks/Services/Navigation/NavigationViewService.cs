using Microsoft.UI.Xaml.Controls;

namespace Steampunks.Services
{
    public class NavigationViewService : INavigationViewService
    {
        private readonly INavigationService _navigationService;
        private NavigationView _navigationView;

        public INavigationService NavigationService => _navigationService;

        public NavigationView NavigationView
        {
            get => _navigationView;
            set
            {
                UnregisterEvents();
                _navigationView = value;
                RegisterEvents();
            }
        }

        public NavigationViewService(INavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        public void Initialize(NavigationView navigationView)
        {
            NavigationView = navigationView;
        }

        public void UnregisterEvents()
        {
            if (_navigationView != null)
            {
                _navigationView.ItemInvoked -= NavigationView_ItemInvoked;
                _navigationView.BackRequested -= NavigationView_BackRequested;
            }
        }

        private void RegisterEvents()
        {
            if (_navigationView != null)
            {
                _navigationView.ItemInvoked += NavigationView_ItemInvoked;
                _navigationView.BackRequested += NavigationView_BackRequested;
            }
        }

        private void NavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
                _navigationService.NavigateTo("settings");
                return;
            }

            var selectedItem = args.InvokedItemContainer as NavigationViewItem;
            if (selectedItem?.GetValue(NavigationHelper.NavigateToProperty) is string pageKey)
            {
                _navigationService.NavigateTo(pageKey);
            }
        }

        private void NavigationView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            _navigationService.GoBack();
        }
    }

    public static class NavigationHelper
    {
        public static readonly Microsoft.UI.Xaml.DependencyProperty NavigateToProperty =
            Microsoft.UI.Xaml.DependencyProperty.RegisterAttached(
                "NavigateTo",
                typeof(string),
                typeof(NavigationHelper),
                new Microsoft.UI.Xaml.PropertyMetadata(null));

        public static string GetNavigateTo(Microsoft.UI.Xaml.DependencyObject obj)
        {
            return (string)obj.GetValue(NavigateToProperty);
        }

        public static void SetNavigateTo(Microsoft.UI.Xaml.DependencyObject obj, string value)
        {
            obj.SetValue(NavigateToProperty, value);
        }
    }
} 