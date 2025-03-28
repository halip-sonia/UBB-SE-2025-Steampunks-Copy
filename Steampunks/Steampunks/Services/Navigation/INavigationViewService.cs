using Microsoft.UI.Xaml.Controls;

namespace Steampunks.Services
{
    public interface INavigationViewService
    {
        INavigationService NavigationService { get; }
        NavigationView NavigationView { get; set; }
        void Initialize(NavigationView navigationView);
        void UnregisterEvents();
    }
} 