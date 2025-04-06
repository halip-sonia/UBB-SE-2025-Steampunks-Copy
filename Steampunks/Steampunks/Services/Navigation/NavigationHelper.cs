namespace Steampunks.Services
{
    using Microsoft.UI.Xaml.Controls;

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