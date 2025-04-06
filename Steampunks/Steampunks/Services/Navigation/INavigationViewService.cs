namespace Steampunks.Services
{
    using Microsoft.UI.Xaml.Controls;

    public interface INavigationViewService
    {
        INavigationService NavigationService { get; }

        NavigationView? NavigationView { get; set; }

        void Initialize(NavigationView navigationView);

        void UnregisterEvents();
    }
}