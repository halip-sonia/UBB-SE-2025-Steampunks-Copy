namespace Steampunks.Services
{
    public static class NavigationHelper
    {
        public static readonly Microsoft.UI.Xaml.DependencyProperty NavigateToProperty =
            Microsoft.UI.Xaml.DependencyProperty.RegisterAttached(
                "NavigateTo",
                typeof(string),
                typeof(NavigationHelper),
                new Microsoft.UI.Xaml.PropertyMetadata(null));

        public static string GetNavigateTo(Microsoft.UI.Xaml.DependencyObject uiTargetElement)
        {
            return (string)uiTargetElement.GetValue(NavigateToProperty);
        }

        public static void SetNavigateTo(Microsoft.UI.Xaml.DependencyObject uiTargetElement, string value)
        {
            uiTargetElement.SetValue(NavigateToProperty, value);
        }
    }
}