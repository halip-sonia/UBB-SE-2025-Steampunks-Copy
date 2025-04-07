namespace Steampunks.Services
{
    using Microsoft.UI.Xaml.Controls;

    public delegate void NavigatedEventHandler(object sender, NavigatedEventArgs e);

    public interface INavigationService
    {
        event NavigatedEventHandler Navigated;

        bool CanGoBack { get; }

        Frame Frame { get; set; }

        bool NavigateTo(string pageKey, object? parameter = null, bool clearNavigation = false);

        bool GoBack();
    }
}