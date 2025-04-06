namespace Steampunks.Services
{
    using System;
    using Microsoft.UI.Xaml.Navigation;

    public class NavigatedEventArgs : EventArgs
    {
        public object? Parameter { get; set; }

        public NavigationMode? NavigationMode { get; set; }

        public Type? SourcePageType { get; set; }
    }
}