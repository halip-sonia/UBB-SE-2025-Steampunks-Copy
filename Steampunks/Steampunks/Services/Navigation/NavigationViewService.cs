namespace Steampunks.Services
{
    using Microsoft.UI.Xaml.Controls;

    public class NavigationViewService : INavigationViewService
    {
        private readonly INavigationService navigationService;
        private NavigationView? navigationView;

        public NavigationViewService(INavigationService navigationService)
        {
            this.navigationService = navigationService;
        }

        public INavigationService NavigationService => this.navigationService;

        public NavigationView? NavigationView
        {
            get => this.navigationView;
            set
            {
                this.UnregisterEvents();
                this.navigationView = value;
                this.RegisterEvents();
            }
        }

        public void Initialize(NavigationView navigationView)
        {
            this.NavigationView = navigationView;
        }

        public void UnregisterEvents()
        {
            if (this.navigationView != null)
            {
                this.navigationView.ItemInvoked -= this.NavigationView_ItemInvoked;
                this.navigationView.BackRequested -= this.NavigationView_BackRequested;
            }
        }

        private void RegisterEvents()
        {
            if (this.navigationView != null)
            {
                this.navigationView.ItemInvoked += this.NavigationView_ItemInvoked;
                this.navigationView.BackRequested += this.NavigationView_BackRequested;
            }
        }

        private void NavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
                this.navigationService.NavigateTo("settings");
                return;
            }

            var selectedItem = args.InvokedItemContainer as NavigationViewItem;
            if (selectedItem?.GetValue(NavigationHelper.NavigateToProperty) is string pageKey)
            {
                this.navigationService.NavigateTo(pageKey);
            }
        }

        private void NavigationView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            this.navigationService.GoBack();
        }
    }
}